// Parallel Planner with Review: plan → build → review → pull request, one round
//
//   Plan:   The host reads the open Sandcastle issues with its own gh auth,
//           keeping only issues and comments from the owner, members and
//           collaborators, and leaving out those handed to a person
//           (sandcastle:needs-human). Issues that already have an open
//           same-repo PR can't be picked, but the planner sees them, since
//           they still block the issues that depend on them. A planner
//           agent picks the ready issues that can be built in parallel, and
//           the host names each issue's branch (lib/branches.mts).
//   Build:  For each issue, in its own sandbox (lib/build.mts): origin/main is
//           merged into the branch, the implementer works the issue, the host
//           runs .sandcastle/check.sh, a reviewer refines the change and
//           returns an approve or reject verdict, and the host checks again
//           if HEAD moved. An approved branch's checked commit is pushed and
//           gets its own draft PR that fixes the issue; anything else gets a
//           comment on the issue and isn't pushed. All pipelines run
//           concurrently.
//
// One round per run: nothing merges into main during a run (each PR waits for
// a person), so a second round couldn't unblock anything, and would only
// retry the issues that stopped. Run it again once PRs have merged.
//
// No issue is closed here: each change reaches main through its PR and the
// checks in docs/PROCESS.md, and the PR's "Fixes #n" closes the issue.
//
// The sandbox gets no GitHub token and no Docker. Agents read the issue from
// their prompt, and every GitHub write (comments, pushes, PRs) is made by the
// host, in code, with git config the sandbox can't reach (lib/git.mts). The
// Docker-backed test suites run in CI.
//
// Usage (from the repo root, with `gh auth login` and `gh auth setup-git` done,
// so git can push over HTTPS):
//   pnpm run sandcastle

import { existsSync, readFileSync } from "node:fs";
import * as sandcastle from "@ai-hero/sandcastle";
import { docker } from "@ai-hero/sandcastle/sandboxes/docker";
import { z } from "zod";
import { prepareBranches, uniqueIssues, withoutOpenPullRequests } from "./lib/branches.mts";
import { buildIssue } from "./lib/build.mts";
import { baseCheck } from "./lib/check.mts";
import { BASE_BRANCH, MODEL } from "./lib/config.mts";
import { fetchFromOrigin, liveRepos, requirePushCredentials } from "./lib/git.mts";
import { listSandcastleIssues, openPullRequestBranches } from "./lib/github.mts";
import { plannerPromptArgs } from "./lib/prompts.mts";
import { githubTokensIn } from "./lib/sandbox-env.mts";

const envFile = ".sandcastle/.env";
const leakedTokens = existsSync(envFile) ? githubTokensIn(readFileSync(envFile, "utf8")) : [];
if (leakedTokens.length > 0) {
  throw new Error(
    `${envFile} sets ${leakedTokens.join(" and ")}, which Sandcastle would pass into the sandbox. ` +
      "Remove it: the host uses its own gh auth, and agents must not reach GitHub.",
  );
}

// The planner emits its plan as JSON inside <plan> tags; Output.object extracts
// and validates it against this schema. There's no branch field: the host
// names branches, so a re-plan can't move an issue's work to a new branch.
const planSchema = z.object({
  issues: z.array(z.object({ id: z.string(), title: z.string() })),
});

async function main(): Promise<void> {
  requirePushCredentials(liveRepos().host);

  // -------------------------------------------------------------------------
  // Plan
  // -------------------------------------------------------------------------
  const { ready, inReview } = withoutOpenPullRequests(listSandcastleIssues(), openPullRequestBranches());
  for (const issue of inReview) {
    console.log(`  ⏸ #${issue.number} is held back: its pull request is open.`);
  }
  if (ready.length === 0) {
    console.log("No open Sandcastle issues ready to build.");
    return;
  }

  const plan = await sandcastle.run({
    sandbox: docker(),
    name: "planner",
    // Structured output requires maxIterations: 1.
    maxIterations: 1,
    agent: sandcastle.claudeCode(MODEL),
    promptFile: "./.sandcastle/plan-prompt.md",
    promptArgs: plannerPromptArgs(ready, inReview),
    // Throws StructuredOutputError if the tag is missing, the JSON is
    // malformed, or validation fails, which ends the run.
    output: sandcastle.Output.object({ tag: "plan", schema: planSchema }),
  });

  // Keep only ids from the ready list, so a hallucinated, stale or in-review
  // id can't start work on an issue that wasn't offered, and each issue only
  // once, so two pipelines never share a branch.
  const picks = uniqueIssues(
    plan.output.issues.flatMap(({ id }) => {
      const issue = ready.find((open) => String(open.number) === id);
      if (!issue) console.warn(`  Skipping ${id}: it isn't one of the ready issues.`);
      return issue ? [issue] : [];
    }),
  );
  if (picks.length === 0) {
    console.log("No unblocked issues to work on.");
    return;
  }

  // -------------------------------------------------------------------------
  // Build, review and publish
  // -------------------------------------------------------------------------
  // Pin the base and its check.sh right after the fetch, from the host-only
  // repo: agents share the clone's refs, so every later decision uses this
  // commit, not a ref.
  const repos = liveRepos();
  const base = baseCheck(fetchFromOrigin(repos, BASE_BRANCH), repos.host);
  const work = prepareBranches(picks);

  console.log(`Planning complete. ${work.length} issue(s) to build in parallel:`);
  for (const { issue, branch } of work) {
    console.log(`  #${issue.number}: ${issue.title} → ${branch}`);
  }

  // Promise.allSettled means one failing pipeline doesn't cancel the others.
  const settled = await Promise.allSettled(work.map(({ issue, branch }) => buildIssue(issue, branch, base)));

  const published: string[] = [];
  for (const [i, outcome] of settled.entries()) {
    const { issue, branch } = work[i]!;
    if (outcome.status === "rejected") {
      console.error(`  ✗ #${issue.number} (${branch}) failed: ${outcome.reason}`);
    } else if (outcome.value.prUrl) {
      published.push(`  #${issue.number} (${branch}) → ${outcome.value.prUrl}`);
    } else {
      console.log(`  #${issue.number} (${branch}): ${outcome.value.outcome}`);
    }
  }

  console.log(`\nDone. ${published.length} pull request(s):`);
  for (const line of published) console.log(line);
}

await main();
