// Parallel Planner with Review — three-phase orchestration loop
//
// This template drives a multi-phase workflow:
//   Phase 1 (Plan):             An opus agent analyzes open issues, builds a
//                               dependency graph, and outputs a <plan> JSON
//                               listing unblocked issues. The host names
//                               each issue's branch (lib/branches.mts).
//   Phase 2 (Execute + Review): For each issue, a sandbox is created via
//                               createSandbox(). The implementer runs first
//                               (100 iterations). If it signals completion and
//                               .sandcastle/check.sh passes in the sandbox, a
//                               reviewer runs in the same sandbox on the same
//                               branch (1 iteration), and the check runs again.
//                               All issue pipelines run concurrently via
//                               Promise.allSettled().
//   Phase 3 (Merge):            A single agent merges the completed branches
//                               (the implementer and reviewer both signalled
//                               completion, and the host's check passed) into
//                               the current branch.
//
// The sandbox has no Docker, on purpose: the host's Docker socket would give
// the agents, who read public issue content, root on the host. So check.sh
// skips the Docker-backed test projects; the host's pre-push gate
// (scripts/gate.sh) and CI run those.
//
// The outer loop repeats up to MAX_ITERATIONS times so that newly unblocked
// issues are picked up after each round of merges. It stops early when a
// round produces no commits at all, since nothing changed to replan.
//
// Usage (from the repo root, on the branch the work should land on):
//   pnpm dlx tsx .sandcastle/main.mts

import { execFileSync } from "node:child_process";
import * as sandcastle from "@ai-hero/sandcastle";
import { docker } from "@ai-hero/sandcastle/sandboxes/docker";
import { z } from "zod";
import { branchFor, localIssueBranches, openSandcastleIssues } from "./lib/branches.mts";
import { runCheck, tail } from "./lib/check.mts";

// The planner emits its plan as JSON inside <plan> tags; Output.object extracts
// and validates it against this schema. We use Zod here, but any Standard
// Schema validator works just as well — Valibot, ArkType, etc. See
// https://standardschema.dev.
const planSchema = z.object({
  issues: z.array(
    z.object({ id: z.string(), title: z.string() }),
  ),
});

// ---------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------

// Maximum number of plan→execute→merge cycles before stopping.
// Raise this if your backlog is large; lower it for a quick smoke-test run.
const MAX_ITERATIONS = 10;

// Hooks run inside the sandbox before the agent starts each iteration.
// pnpm (the version pinned by package.json's packageManager, enabled in the
// image through corepack) installs exactly what pnpm-lock.yaml records, and
// fails rather than re-resolve when the lockfile is out of date.
const hooks = {
  sandbox: { onSandboxReady: [{ command: "pnpm install --frozen-lockfile" }] },
};

// Copy node_modules from the host into the worktree before each sandbox
// starts. Avoids a full install from scratch; the hook above handles
// platform-specific binaries and any packages added since the last copy.
const copyToWorktree = ["node_modules"];

// What the implementer and reviewer print once their work is done and
// .sandcastle/check.sh passes (see implement-prompt.md and review-prompt.md).
// The host runs the check itself before it believes them.
const COMPLETE = "<promise>COMPLETE</promise>";

// The branch the run started on: issue branches are cut from it, the reviewer
// diffs against it, and the merger merges into it. Sandcastle's built-in
// {{TARGET_BRANCH}} can't serve here, since inside a createSandbox() sandbox
// it names the issue branch itself, so the review prompt uses BASE_BRANCH.
const baseBranch = execFileSync("git", ["rev-parse", "--abbrev-ref", "HEAD"], {
  encoding: "utf8",
}).trim();
if (baseBranch === "HEAD") {
  throw new Error("Run Sandcastle from a branch, not a detached HEAD.");
}

// Runs .sandcastle/check.sh in the issue's sandbox and logs a failure's last
// lines. Its exit code, not an agent's completion signal, decides.
async function checkPasses(
  sandbox: Parameters<typeof runCheck>[0],
  issue: { id: string; branch: string },
  when: string,
): Promise<boolean> {
  const check = await runCheck(sandbox);
  if (!check.passed) {
    console.log(`  ${issue.id} (${issue.branch}): .sandcastle/check.sh failed ${when}:\n${tail(check.output, 40)}`);
  }
  return check.passed;
}

// ---------------------------------------------------------------------------
// Main loop
// ---------------------------------------------------------------------------

for (let iteration = 1; iteration <= MAX_ITERATIONS; iteration++) {
  console.log(`\n=== Iteration ${iteration}/${MAX_ITERATIONS} ===\n`);

  // -------------------------------------------------------------------------
  // Phase 1: Plan
  //
  // The planning agent (opus, for deeper reasoning) reads the open issue list,
  // builds a dependency graph, and selects the issues that can be worked in
  // parallel right now (i.e., no blocking dependencies on other open issues).
  //
  // It outputs a <plan> JSON block — Output.object parses and validates it.
  // -------------------------------------------------------------------------
  const plan = await sandcastle.run({
    hooks,
    sandbox: docker(),
    name: "planner",
    // One iteration is enough: the planner just needs to read and reason,
    // not write code. (Structured output requires maxIterations: 1.)
    maxIterations: 1,
    // Opus for planning: dependency analysis benefits from deeper reasoning.
    agent: sandcastle.claudeCode("claude-opus-4-8"),
    promptFile: "./.sandcastle/plan-prompt.md",
    // Extract and validate the <plan> JSON into a typed object. Throws
    // StructuredOutputError if the tag is missing, the JSON is malformed, or
    // validation fails — which aborts the loop.
    output: sandcastle.Output.object({ tag: "plan", schema: planSchema }),
  });

  // Name each picked issue's branch from its number, title and labels on
  // GitHub, so the name follows the branch standard and doesn't drift
  // between plans. An id that isn't an open Sandcastle issue is skipped.
  const openIssues = openSandcastleIssues();
  const existingBranches = localIssueBranches();
  const issues = plan.output.issues.flatMap(({ id }) => {
    const issue = openIssues.find((open) => String(open.number) === id);
    if (!issue) {
      console.warn(`  Skipping ${id}: not an open issue labelled Sandcastle.`);
      return [];
    }
    return [{ id, title: issue.title, branch: branchFor(issue, existingBranches) }];
  });

  if (issues.length === 0) {
    // No unblocked work — either everything is done or everything is blocked.
    console.log("No unblocked issues to work on. Exiting.");
    break;
  }

  console.log(
    `Planning complete. ${issues.length} issue(s) to work in parallel:`,
  );
  for (const issue of issues) {
    console.log(`  ${issue.id}: ${issue.title} → ${issue.branch}`);
  }

  // -------------------------------------------------------------------------
  // Phase 2: Execute + Review
  //
  // For each issue, create a sandbox via createSandbox() so the implementer
  // and reviewer share the same sandbox instance per branch. The implementer
  // runs first; if it signals completion, the reviewer runs in the same sandbox.
  //
  // Promise.allSettled means one failing pipeline doesn't cancel the others.
  // -------------------------------------------------------------------------

  const settled = await Promise.allSettled(
    issues.map(async (issue) => {
      const sandbox = await sandcastle.createSandbox({
        branch: issue.branch,
        baseBranch,
        sandbox: docker(),
        hooks,
        copyToWorktree,
      });

      try {
        // Run the implementer
        const implement = await sandbox.run({
          name: "implementer",
          maxIterations: 100,
          agent: sandcastle.claudeCode("claude-opus-4-8"),
          promptFile: "./.sandcastle/implement-prompt.md",
          promptArgs: {
            TASK_ID: issue.id,
            ISSUE_TITLE: issue.title,
            BRANCH: issue.branch,
          },
        });

        // The implementer may commit partial work and stop without
        // finishing (out of iterations, or blocked). Only a run that printed
        // the completion signal, and whose branch then passes the host's
        // check, is reviewed and counted complete. Partial work stays on
        // the issue's branch, and a later round picks the branch up again.
        if (implement.completionSignal !== COMPLETE) {
          return { commits: implement.commits, complete: false };
        }
        if (!(await checkPasses(sandbox, issue, "after the implementer"))) {
          return { commits: implement.commits, complete: false };
        }

        const review = await sandbox.run({
          name: "reviewer",
          maxIterations: 1,
          agent: sandcastle.claudeCode("claude-opus-4-8"),
          promptFile: "./.sandcastle/review-prompt.md",
          promptArgs: {
            BRANCH: issue.branch,
            BASE_BRANCH: baseBranch,
          },
        });

        // Merge commits from both runs: each sandbox.run() only returns
        // commits from its own run. A reviewer that committed changed the
        // branch, so the host checks it again.
        const commits = [...implement.commits, ...review.commits];
        const complete =
          review.completionSignal === COMPLETE &&
          (review.commits.length === 0 || (await checkPasses(sandbox, issue, "after the reviewer")));
        return { commits, complete };
      } finally {
        await sandbox.close();
      }
    }),
  );

  // Log any agents that threw (network error, sandbox crash, etc.).
  for (const [i, outcome] of settled.entries()) {
    if (outcome.status === "rejected") {
      console.error(
        `  ✗ ${issues[i]!.id} (${issues[i]!.branch}) failed: ${outcome.reason}`,
      );
    }
  }

  const results = settled.map((outcome, i) => ({
    issue: issues[i]!,
    commits: outcome.status === "fulfilled" ? outcome.value.commits.length : 0,
    complete: outcome.status === "fulfilled" && outcome.value.complete,
  }));

  if (results.every((result) => result.commits === 0)) {
    // No pipeline changed anything, so the next plan would pick the same
    // issues and repeat the same runs. Stop rather than burn iterations.
    console.log("\nNo commits produced this round. Stopping.");
    break;
  }

  // Only branches whose implementer and reviewer both signalled completion
  // go to the merge phase, which closes their issues.
  const completedIssues = results
    .filter((result) => result.complete && result.commits > 0)
    .map((result) => result.issue);

  for (const result of results) {
    if (result.commits > 0 && !result.complete) {
      console.log(`  ${result.issue.id} (${result.issue.branch}) is not complete; left unmerged.`);
    }
  }

  const completedBranches = completedIssues.map((i) => i.branch);

  console.log(
    `\nExecution complete. ${completedBranches.length} completed branch(es):`,
  );
  for (const branch of completedBranches) {
    console.log(`  ${branch}`);
  }

  if (completedBranches.length === 0) {
    // Some work was committed but nothing finished: the next round picks
    // the unfinished branches up again.
    console.log("Nothing complete to merge this round.");
    continue;
  }

  // -------------------------------------------------------------------------
  // Phase 3: Merge
  //
  // One agent merges all completed branches into the current branch,
  // resolving any conflicts and running .sandcastle/check.sh to confirm
  // everything works.
  //
  // The {{BRANCHES}} and {{ISSUES}} prompt arguments are lists that the agent
  // uses to know which branches to merge and which issues to close.
  // -------------------------------------------------------------------------
  await sandcastle.run({
    hooks,
    sandbox: docker(),
    name: "merger",
    maxIterations: 1,
    agent: sandcastle.claudeCode("claude-opus-4-8"),
    promptFile: "./.sandcastle/merge-prompt.md",
    promptArgs: {
      // A markdown list of branch names, one per line.
      BRANCHES: completedBranches.map((b) => `- ${b}`).join("\n"),
      // A markdown list of issue IDs and titles, one per line.
      ISSUES: completedIssues.map((i) => `- ${i.id}: ${i.title}`).join("\n"),
    },
  });

  console.log("\nBranches merged.");
}

console.log("\nAll done.");
