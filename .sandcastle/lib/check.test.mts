import assert from "node:assert/strict";
import { describe, it } from "node:test";
import { runCheck, tail } from "./check.mts";

type Result = { stdout: string; stderr: string; exitCode: number };

// A sandbox whose exec answers each command from a table; any other command fails the test.
const sandboxWith = (results: Record<string, { stdout: string; exitCode: number }>) => ({
  exec: async (command: string): Promise<Result> => {
    const result = results[command];
    if (!result) throw new Error(`unexpected command: ${command}`);
    return { ...result, stderr: "" };
  },
});

describe("runCheck", () => {
  it("passes when the check passes and the worktree is clean", async () => {
    const result = await runCheck(
      sandboxWith({
        ".sandcastle/check.sh 2>&1": { stdout: "ok\n", exitCode: 0 },
        "git status --porcelain 2>&1": { stdout: "", exitCode: 0 },
      }),
    );
    assert.deepEqual(result, { passed: true, output: "ok\n" });
  });

  it("fails when the check fails, without looking at the worktree", async () => {
    const result = await runCheck(sandboxWith({ ".sandcastle/check.sh 2>&1": { stdout: "error CS1002", exitCode: 1 } }));
    assert.deepEqual(result, { passed: false, output: "error CS1002" });
  });

  it("fails when the check passes over uncommitted changes", async () => {
    const result = await runCheck(
      sandboxWith({
        ".sandcastle/check.sh 2>&1": { stdout: "ok", exitCode: 0 },
        "git status --porcelain 2>&1": { stdout: " M src/Api/Program.cs\n", exitCode: 0 },
      }),
    );
    assert.equal(result.passed, false);
    assert.match(result.output, /uncommitted changes:\n M src\/Api\/Program\.cs/);
  });

  it("fails when git status fails", async () => {
    const result = await runCheck(
      sandboxWith({
        ".sandcastle/check.sh 2>&1": { stdout: "ok", exitCode: 0 },
        "git status --porcelain 2>&1": { stdout: "fatal: not a git repository", exitCode: 128 },
      }),
    );
    assert.equal(result.passed, false);
    assert.match(result.output, /git status failed/);
  });
});

describe("tail", () => {
  it("keeps the last lines, ignoring a trailing newline", () => {
    assert.equal(tail("a\nb\nc\n", 2), "b\nc");
  });

  it("keeps everything when there are fewer lines", () => {
    assert.equal(tail("a", 5), "a");
  });
});
