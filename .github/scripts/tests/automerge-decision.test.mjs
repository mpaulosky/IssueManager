// Tests for .github/scripts/automerge-decision.mjs.
// Usage: node --test .github/scripts/tests/
import { test } from "node:test";
import assert from "node:assert/strict";
import { decideAutoMerge } from "../automerge-decision.mjs";

// An open, same-repo, non-draft PR that Copilot has reviewed at its head, with
// no open review threads, whose required checks have passed.
const pr = (overrides = {}) => ({
  state: "OPEN",
  merged: false,
  isDraft: false,
  isSameRepo: true,
  mergeable: "MERGEABLE",
  mergeStateStatus: "CLEAN",
  autoMergeEnabled: false,
  copilotReviewedHead: true,
  unresolvedThreads: 0,
  threadsTruncated: false,
  ...overrides,
});

test("merges a clean PR that Copilot reviewed at its head and whose threads are all resolved", () => {
  assert.equal(decideAutoMerge(pr()).action, "merge");
});

test("merges when only optional checks are pending or failing", () => {
  assert.equal(decideAutoMerge(pr({ mergeStateStatus: "UNSTABLE" })).action, "merge");
});

test("waits for Copilot's review of the head commit", () => {
  const decision = decideAutoMerge(pr({ copilotReviewedHead: false }));
  assert.equal(decision.action, "wait");
  assert.match(decision.reason, /Copilot/);
});

test("waits for Copilot even when the required checks have already passed", () => {
  for (const status of ["CLEAN", "UNSTABLE"]) {
    assert.equal(decideAutoMerge(pr({ mergeStateStatus: status, copilotReviewedHead: false })).action, "wait");
  }
});

test("waits while a review thread is unresolved", () => {
  const decision = decideAutoMerge(pr({ unresolvedThreads: 2 }));
  assert.equal(decision.action, "wait");
  assert.match(decision.reason, /2 unresolved/);
});

test("leaves a PR with more review threads than one page to a person", () => {
  assert.equal(decideAutoMerge(pr({ threadsTruncated: true })).action, "skip");
});

test("waits on a PR still waiting for required checks", () => {
  assert.equal(decideAutoMerge(pr({ mergeStateStatus: "BLOCKED" })).action, "wait");
});

test("waits on a PR that is behind main", () => {
  assert.equal(decideAutoMerge(pr({ mergeStateStatus: "BEHIND" })).action, "wait");
});

test("waits while GitHub is still computing mergeability", () => {
  assert.equal(decideAutoMerge(pr({ mergeable: "UNKNOWN", mergeStateStatus: "UNKNOWN" })).action, "wait");
});

test("never arms GitHub's auto-merge, which would merge before the review", () => {
  for (const status of ["BLOCKED", "BEHIND", "UNKNOWN", "CLEAN", "UNSTABLE", "DIRTY"]) {
    for (const copilotReviewedHead of [true, false]) {
      const { action } = decideAutoMerge(pr({ mergeStateStatus: status, copilotReviewedHead }));
      assert.notEqual(action, "enable", `${status} reviewed=${copilotReviewedHead}`);
    }
  }
});

test("skips a PR that already has auto-merge armed, such as a release blog PR", () => {
  assert.equal(decideAutoMerge(pr({ autoMergeEnabled: true })).action, "skip");
});

test("skips a draft PR", () => {
  assert.equal(decideAutoMerge(pr({ isDraft: true })).action, "skip");
});

test("skips a PR from a fork", () => {
  assert.equal(decideAutoMerge(pr({ isSameRepo: false })).action, "skip");
});

test("skips a PR with merge conflicts", () => {
  assert.equal(decideAutoMerge(pr({ mergeable: "CONFLICTING", mergeStateStatus: "DIRTY" })).action, "skip");
});

test("skips a closed or already-merged PR", () => {
  assert.equal(decideAutoMerge(pr({ state: "CLOSED" })).action, "skip");
  assert.equal(decideAutoMerge(pr({ state: "MERGED", merged: true })).action, "skip");
});

test("gives a reason for every decision", () => {
  const cases = [
    pr(),
    pr({ mergeStateStatus: "BLOCKED" }),
    pr({ mergeStateStatus: "DIRTY" }),
    pr({ copilotReviewedHead: false }),
    pr({ unresolvedThreads: 1 }),
    pr({ threadsTruncated: true }),
  ];
  for (const input of cases) {
    const { reason } = decideAutoMerge(input);
    assert.ok(reason && reason.length > 0, `missing reason for ${JSON.stringify(input)}`);
  }
});
