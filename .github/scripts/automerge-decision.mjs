// Decides what the PR auto-merge workflow should do with a pull request.
//
// A PR merges once it is ready: its required checks have passed, Copilot has
// reviewed its head commit, and every review thread is resolved. The main
// ruleset asks Copilot to review each push but doesn't require its review, so
// merging on green checks alone would usually land a PR before that review
// arrives. For the same reason GitHub's own auto-merge is never armed here: it
// merges the moment the required checks pass. The workflow re-checks each PR
// on every event that could make it ready, and on a schedule.
//
// A PR whose required checks passed while only optional ones are pending or
// failing (UNSTABLE) merges too, as it did before this gate.

/**
 * @param {{
 *   state: string,               // GraphQL PullRequestState: OPEN | CLOSED | MERGED
 *   merged: boolean,
 *   isDraft: boolean,
 *   isSameRepo: boolean,         // head branch lives in this repository
 *   mergeable: string,           // MERGEABLE | CONFLICTING | UNKNOWN
 *   mergeStateStatus: string,    // CLEAN | BLOCKED | UNSTABLE | BEHIND | DIRTY | UNKNOWN | ...
 *   autoMergeEnabled: boolean,
 *   copilotReviewedHead: boolean, // a Copilot review exists for the PR's head commit
 *   unresolvedThreads: number,   // unresolved review threads among those fetched
 *   threadsTruncated: boolean,   // more threads exist than one page fetched
 * }} pr
 * @returns {{ action: "skip" | "wait" | "merge", reason: string }}
 */
export function decideAutoMerge(pr) {
  if (pr.state !== "OPEN" || pr.merged) {
    return { action: "skip", reason: `PR is ${pr.merged ? "merged" : pr.state.toLowerCase()}` };
  }
  if (pr.isDraft) {
    return { action: "skip", reason: "PR is a draft" };
  }
  if (!pr.isSameRepo) {
    return { action: "skip", reason: "PR comes from a fork" };
  }
  // Two workflows arm GitHub's auto-merge on purpose, and GitHub finishes
  // those PRs once the required checks pass: release.yml on its blog PRs, and
  // dependabot-auto-merge.yml on Dependabot's, which merge without Copilot's
  // review as they did before this gate.
  if (pr.autoMergeEnabled) {
    return { action: "skip", reason: "auto-merge is already armed" };
  }
  if (pr.mergeable === "CONFLICTING" || pr.mergeStateStatus === "DIRTY") {
    return { action: "skip", reason: "PR has merge conflicts" };
  }
  // Copilot re-reviews every push, including merge-from-main commits, so its
  // review has to cover the exact head.
  if (!pr.copilotReviewedHead) {
    return { action: "wait", reason: "no Copilot review of the head commit yet" };
  }
  // Only one page of threads is fetched; past that, leave the merge to a
  // person rather than miss an unresolved one.
  if (pr.threadsTruncated) {
    return { action: "skip", reason: "more review threads than one page; merge it by hand" };
  }
  if (pr.unresolvedThreads > 0) {
    return { action: "wait", reason: `${pr.unresolvedThreads} unresolved review thread(s)` };
  }
  if (pr.mergeStateStatus === "CLEAN") {
    return { action: "merge", reason: "PR meets every requirement" };
  }
  if (pr.mergeStateStatus === "UNSTABLE") {
    return { action: "merge", reason: "required checks passed; only optional checks are pending or failing" };
  }
  return { action: "wait", reason: `waiting on requirements (mergeStateStatus=${pr.mergeStateStatus})` };
}
