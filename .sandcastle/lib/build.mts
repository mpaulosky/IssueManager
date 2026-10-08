// Build one planned issue in a single sandbox: implement, check, review, check
// again, and publish it as its own pull request. Nothing is merged into main
// and no issue is closed here: the PR says "Fixes #n", so the issue closes
// when the PR merges through the normal checks and review in docs/PROCESS.md.

import * as sandcastle from "@ai-hero/sandcastle";
import { docker } from "@ai-hero/sandcastle/sandboxes/docker";
import { commitsAhead, containsBase, headOf, mergeBase, pushChecked } from "./branches.mts";
import { fenced, runCheck, tail } from "./check.mts";
import { BASE_BRANCH, CHECK_COMMENT_LINES, copyToWorktree, hooks, IMPLEMENTER_ITERATIONS, MODEL } from "./config.mts";
import { commentOnIssue, openPullRequest, type SandcastleIssue } from "./github.mts";
import { issuePromptArgs } from "./prompts.mts";
import { prBody, prTitle } from "./publish.mts";
import { parseVerdict, type Verdict } from "./verdict.mts";

// The parts of a sandbox buildIssue uses; tests pass a fake.
export type BuildSandbox = Pick<sandcastle.Sandbox, "run" | "exec" | "close" | "worktreePath">;

// What buildIssue needs from outside the pipeline; tests pass stubs.
export type BuildHost = {
  createSandbox(branch: string): Promise<BuildSandbox>;
  // Merge the base branch into the worktree's branch; false when it conflicts.
  mergeBase(worktreePath: string): boolean;
  head(worktreePath: string): { sha: string; branch: string };
  commitsAhead(worktreePath: string, sha: string): number;
  containsBase(worktreePath: string, sha: string): boolean;
  commentOnIssue(issueNumber: number, body: string): void;
  // Push the checked commit to the branch and open (or reuse) its pull request.
  publish(worktreePath: string, sha: string, branch: string, title: string, body: string): string;
  log(line: string): void;
};

const liveHost: BuildHost = {
  createSandbox: (branch) =>
    sandcastle.createSandbox({ branch, baseBranch: BASE_BRANCH, sandbox: docker(), hooks, copyToWorktree }),
  mergeBase,
  head: headOf,
  commitsAhead,
  containsBase,
  commentOnIssue,
  publish: (worktreePath, sha, branch, title, body) => {
    pushChecked(worktreePath, sha, branch);
    return openPullRequest(branch, title, body);
  },
  log: console.log,
};

export type BuildOutcome =
  | "published"
  | "nothing-to-publish"
  | "implementer-unfinished"
  | "wrong-branch"
  | "behind-base"
  | "check-failed"
  | "rejected"
  | "publish-failed";

export async function buildIssue(
  issue: SandcastleIssue,
  branch: string,
  host: BuildHost = liveHost,
): Promise<{ outcome: BuildOutcome; prUrl?: string }> {
  const log = (line: string) => host.log(`  #${issue.number} ${line}`);
  const stop = (outcome: BuildOutcome, comment: string) => {
    log(`stopped: ${outcome}`);
    host.commentOnIssue(issue.number, comment);
    return { outcome };
  };
  const notPushed = `\`${branch}\` wasn't pushed; the local branch keeps its commits.`;
  const promptArgs = issuePromptArgs(issue, branch);

  const sandbox = await host.createSandbox(branch);
  const worktree = sandbox.worktreePath;
  try {
    // Bring an existing branch up to date first, so the work is built on
    // current main. A conflict is left to the implementer, which the prompt
    // asks to merge the base branch when it's behind.
    if (!host.mergeBase(worktree)) log(`merging ${BASE_BRANCH} conflicts; left to the implementer`);

    // Implement. A run that throws or uses up its iterations without
    // signalling completion stops the issue for this round.
    let finished: boolean;
    let failure = "it ran out of iterations unfinished";
    try {
      const implement = await sandbox.run({
        name: "implementer",
        agent: sandcastle.claudeCode(MODEL),
        maxIterations: IMPLEMENTER_ITERATIONS,
        promptFile: "./.sandcastle/implement-prompt.md",
        promptArgs,
      });
      finished = implement.completionSignal !== undefined;
    } catch (error) {
      finished = false;
      failure = `it failed: ${error}`;
    }
    if (!finished) return stop("implementer-unfinished", `Sandcastle stopped building this issue: the implementer ${failure}. ${notPushed}`);

    // Decide from the commit the worktree has checked out, never from what a
    // run reports: a run's commits are only that run's, and an agent can
    // reset or switch branches. Returns the commit that passed the check, or
    // the outcome that stops the issue.
    const verify = async (when: string, checked?: string): Promise<{ sha: string } | { outcome: BuildOutcome }> => {
      const head = host.head(worktree);
      if (head.branch !== branch) {
        return stop("wrong-branch", `Sandcastle stopped building this issue: ${when}, the worktree was on \`${head.branch}\`, not \`${branch}\`. ${notPushed}`);
      }
      if (head.sha === checked) return { sha: head.sha };

      // Whenever the branch holds work main doesn't, not only when this
      // round added commits: a re-run of a finished issue makes none, and
      // its earlier work still needs a PR.
      if (host.commitsAhead(worktree, head.sha) === 0) {
        log(`nothing to publish ${when}`);
        return { outcome: "nothing-to-publish" };
      }
      if (!host.containsBase(worktree, head.sha)) {
        return stop("behind-base", `Sandcastle stopped building this issue: ${when}, \`${branch}\` doesn't contain \`${BASE_BRANCH}\`, and merging it conflicted. ${notPushed}`);
      }

      const check = await runCheck(sandbox, BASE_BRANCH);
      const after = host.head(worktree);
      const moved = after.sha !== head.sha || after.branch !== branch;
      const passed = check.passed && !moved;
      log(`check ${when}: ${passed ? "passed" : "failed"}`);
      if (passed) return { sha: head.sha };
      const output = moved ? `${check.output}\nThe branch moved while the check ran.` : check.output;
      return stop(
        "check-failed",
        `Sandcastle stopped building this issue: \`.sandcastle/check.sh\` failed ${when}. ${notPushed}\n\n` +
          `The last ${CHECK_COMMENT_LINES} lines of its output:\n\n${fenced(tail(output, CHECK_COMMENT_LINES))}`,
      );
    };

    const implemented = await verify("after the implementer");
    if ("outcome" in implemented) return implemented;

    // Review. The reviewer may commit refinements, and must end with a
    // verdict; anything but an approval keeps the branch from being published.
    let verdict: Verdict;
    try {
      const review = await sandbox.run({
        name: "reviewer",
        agent: sandcastle.claudeCode(MODEL),
        maxIterations: 1,
        promptFile: "./.sandcastle/review-prompt.md",
        promptArgs,
      });
      verdict = parseVerdict(review.stdout);
    } catch (error) {
      verdict = { approved: false, summary: `The reviewer failed: ${error}` };
    }
    log(`reviewer ${verdict.approved ? "approved" : "rejected"}`);
    if (!verdict.approved) {
      return stop("rejected", `Sandcastle's reviewer rejected this issue's change, so ${notPushed}\n\n${verdict.summary}`);
    }

    // Check again whatever the reviewer did to HEAD, committed or not.
    const reviewed = await verify("after the reviewer", implemented.sha);
    if ("outcome" in reviewed) return reviewed;

    // Publish while the worktree still exists; close() may remove it.
    try {
      const prUrl = host.publish(worktree, reviewed.sha, branch, prTitle(issue), prBody(issue, verdict.summary));
      log(`published ${prUrl}`);
      return { outcome: "published", prUrl };
    } catch (error) {
      return stop("publish-failed", `Sandcastle couldn't publish \`${branch}\`: ${error}`);
    }
  } finally {
    await sandbox.close();
  }
}
