import assert from "node:assert/strict";
import { describe, it } from "node:test";
import { buildIssue, type BuildHost, type BuildSandbox } from "./build.mts";

const issue = { number: 7, title: "Add search", body: "Search issues.", labels: ["Sandcastle"], comments: [] };
const branch = "feature/7-add-search";

type RunResult = { completionSignal?: string; commits?: { sha: string }[]; stdout?: string } | Error;
type Head = { sha: string; branch: string };

// A pipeline with scripted agent runs, check results and worktree HEADs,
// recording what the host was asked to do. An agent run may move HEAD.
function pipeline(options: {
  implementer?: RunResult;
  reviewer?: RunResult;
  checks?: boolean[];
  // HEAD after each agent run, by run name; HEAD otherwise stays put.
  headAfter?: Record<string, Head>;
  ahead?: number;
  containsBase?: boolean;
  mergeBase?: boolean;
  publishError?: Error;
}) {
  const checks = [...(options.checks ?? [true, true])];
  const calls = {
    runs: [] as string[],
    comments: [] as string[],
    published: [] as { sha: string; title: string; body: string }[],
    merged: 0,
    closed: false,
  };
  let head: Head = { sha: "c1", branch };
  const results: Record<string, RunResult> = {
    implementer: options.implementer ?? { completionSignal: "<promise>COMPLETE</promise>", commits: [{ sha: "c1" }] },
    reviewer: options.reviewer ?? { stdout: '<verdict>{"approved": true, "summary": "Clean."}</verdict>', commits: [] },
  };

  const sandbox = {
    worktreePath: "/tmp/worktree",
    run: async (opts: { name?: string }) => {
      calls.runs.push(opts.name!);
      const result = results[opts.name!]!;
      if (result instanceof Error) throw result;
      head = options.headAfter?.[opts.name!] ?? head;
      return { iterations: [], stdout: "", commits: [], ...result };
    },
    exec: async (command: string) => {
      if (command.includes(".sandcastle/check.sh | bash")) {
        const passed = checks.shift();
        if (passed === undefined) throw new Error("check ran more often than scripted");
        return { stdout: passed ? "ok" : "error CS1002", stderr: "", exitCode: passed ? 0 : 1 };
      }
      return { stdout: "", stderr: "", exitCode: 0 };
    },
    close: async () => {
      calls.closed = true;
      return {};
    },
  } as unknown as BuildSandbox;

  const host: BuildHost = {
    createSandbox: async () => sandbox,
    mergeBase: () => {
      calls.merged++;
      return options.mergeBase ?? true;
    },
    head: () => head,
    commitsAhead: () => options.ahead ?? 1,
    containsBase: () => options.containsBase ?? true,
    commentOnIssue: (_, body) => calls.comments.push(body),
    publish: (_, sha, __, title, body) => {
      if (options.publishError) throw options.publishError;
      calls.published.push({ sha, title, body });
      return "https://github.com/o/r/pull/1";
    },
    log: () => {},
  };

  return { run: () => buildIssue(issue, branch, host), calls, checksLeft: checks };
}

describe("buildIssue", () => {
  it("publishes the checked commit of an approved branch as a PR that fixes the issue", async () => {
    const { run, calls } = pipeline({});
    const result = await run();
    assert.deepEqual(result, { outcome: "published", prUrl: "https://github.com/o/r/pull/1" });
    assert.deepEqual(calls.runs, ["implementer", "reviewer"]);
    assert.equal(calls.merged, 1);
    assert.equal(calls.published[0]!.sha, "c1");
    assert.equal(calls.published[0]!.title, "feat: Add search");
    assert.match(calls.published[0]!.body, /Fixes #7/);
    assert.deepEqual(calls.comments, []);
    assert.ok(calls.closed);
  });

  it("publishes a branch finished in an earlier round, though this round made no commits", async () => {
    const { run, calls } = pipeline({ implementer: { completionSignal: "<promise>COMPLETE</promise>", commits: [] }, ahead: 2 });
    assert.equal((await run()).outcome, "published");
    assert.equal(calls.published.length, 1);
  });

  it("doesn't publish when the reviewer rejects, and says why on the issue", async () => {
    const { run, calls } = pipeline({
      reviewer: { stdout: '<verdict>{"approved": false, "summary": "No test for an empty query."}</verdict>' },
    });
    assert.equal((await run()).outcome, "rejected");
    assert.deepEqual(calls.published, []);
    assert.match(calls.comments[0]!, /No test for an empty query/);
  });

  it("treats a review without a verdict as a rejection", async () => {
    const { run, calls } = pipeline({ reviewer: { stdout: "<promise>COMPLETE</promise>" } });
    assert.equal((await run()).outcome, "rejected");
    assert.deepEqual(calls.published, []);
  });

  it("treats a reviewer that throws as a rejection", async () => {
    const { run, calls } = pipeline({ reviewer: new Error("sandbox crashed") });
    assert.equal((await run()).outcome, "rejected");
    assert.match(calls.comments[0]!, /sandbox crashed/);
  });

  it("stops when the implementer doesn't finish", async () => {
    const { run, calls } = pipeline({ implementer: { commits: [{ sha: "a" }] } });
    assert.equal((await run()).outcome, "implementer-unfinished");
    assert.deepEqual(calls.runs, ["implementer"]);
    assert.match(calls.comments[0]!, /ran out of iterations/);
  });

  it("stops when the check fails after the implementer", async () => {
    const { run, calls } = pipeline({ checks: [false] });
    assert.equal((await run()).outcome, "check-failed");
    assert.deepEqual(calls.runs, ["implementer"]);
    assert.match(calls.comments[0]!, /error CS1002/);
  });

  it("checks again when the reviewer moves HEAD, and stops when that fails", async () => {
    const { run, calls } = pipeline({ headAfter: { reviewer: { sha: "c2", branch } }, checks: [true, false] });
    assert.equal((await run()).outcome, "check-failed");
    assert.deepEqual(calls.published, []);
  });

  it("checks again when the reviewer moves HEAD without reporting a commit, and publishes the new commit", async () => {
    const { run, calls, checksLeft } = pipeline({
      reviewer: { stdout: '<verdict>{"approved": true, "summary": "Tidied."}</verdict>', commits: [] },
      headAfter: { reviewer: { sha: "c2", branch } },
    });
    assert.equal((await run()).outcome, "published");
    assert.equal(checksLeft.length, 0);
    assert.equal(calls.published[0]!.sha, "c2");
  });

  it("doesn't check again when HEAD hasn't moved since the check", async () => {
    const { run, checksLeft } = pipeline({ checks: [true, true] });
    assert.equal((await run()).outcome, "published");
    assert.equal(checksLeft.length, 1);
  });

  it("stops when an agent leaves the worktree on another branch", async () => {
    const { run, calls } = pipeline({ headAfter: { reviewer: { sha: "c1", branch: "main" } } });
    assert.equal((await run()).outcome, "wrong-branch");
    assert.deepEqual(calls.published, []);
    assert.match(calls.comments[0]!, /was on `main`/);
  });

  it("stops when the branch is behind the base and couldn't be merged", async () => {
    const { run, calls } = pipeline({ mergeBase: false, containsBase: false });
    assert.equal((await run()).outcome, "behind-base");
    assert.deepEqual(calls.runs, ["implementer"]);
    assert.deepEqual(calls.published, []);
  });

  it("publishes nothing when the branch has no work main lacks", async () => {
    const { run, calls } = pipeline({ ahead: 0 });
    assert.equal((await run()).outcome, "nothing-to-publish");
    assert.deepEqual(calls.runs, ["implementer"]);
    assert.deepEqual(calls.comments, []);
  });

  it("reports a failed push on the issue", async () => {
    const { run, calls } = pipeline({ publishError: new Error("rejected: fetch first") });
    assert.equal((await run()).outcome, "publish-failed");
    assert.match(calls.comments[0]!, /fetch first/);
    assert.ok(calls.closed);
  });
});
