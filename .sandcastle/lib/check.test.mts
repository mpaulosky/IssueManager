import assert from "node:assert/strict";
import { execFileSync } from "node:child_process";
import { mkdirSync, mkdtempSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { describe, it } from "node:test";
import { baseCheck, checkFiles, headOf, runCheck, shellQuote, tail } from "./check.mts";

type Result = { stdout: string; stderr: string; exitCode: number };

const base = { sha: "abc123", script: "echo it's checked\n" };
const diff = `git diff --name-only abc123...HEAD -- ${checkFiles.join(" ")} 2>&1`;
const check = `bash -c 'echo it'\\''s checked\n' check.sh </dev/null 2>&1`;
const status = "git status --porcelain 2>&1";

// A sandbox whose exec answers each command from a table; any other command fails the test.
const sandboxWith = (results: Record<string, { stdout: string; exitCode: number }>) => ({
  exec: async (command: string): Promise<Result> => {
    const result = results[command];
    if (!result) throw new Error(`unexpected command: ${command}`);
    return { ...result, stderr: "" };
  },
});

const unchanged = { stdout: "", exitCode: 0 };

describe("runCheck", () => {
  it("runs the base's script, passed in as text, and passes when it passes over a clean worktree", async () => {
    const result = await runCheck(
      sandboxWith({ [diff]: unchanged, [check]: { stdout: "ok\n", exitCode: 0 }, [status]: { stdout: "", exitCode: 0 } }),
      base,
    );
    assert.deepEqual(result, { passed: true, output: "ok\n" });
  });

  it("fails when the check fails, without looking at the worktree", async () => {
    const result = await runCheck(sandboxWith({ [diff]: unchanged, [check]: { stdout: "error CS1002", exitCode: 1 } }), base);
    assert.deepEqual(result, { passed: false, output: "error CS1002" });
  });

  it("fails, without running it, and asks for a person when the branch changes the check's own files", async () => {
    const result = await runCheck(sandboxWith({ [diff]: { stdout: "package.json\n", exitCode: 0 } }), base);
    assert.equal(result.passed, false);
    assert.equal(result.needsHuman, true);
    assert.match(result.output, /changes the check's own files.*\npackage\.json/);
  });

  it("protects every file check.sh runs from the worktree", () => {
    assert.deepEqual(
      [...checkFiles].sort(),
      [".github/ci/gate-checks.sh", ".github/scripts/discover_tests.py", ".sandcastle/check.sh", "package.json"],
    );
  });

  it("fails when git diff fails", async () => {
    const result = await runCheck(sandboxWith({ [diff]: { stdout: "fatal: bad revision", exitCode: 128 } }), base);
    assert.equal(result.passed, false);
    assert.equal(result.needsHuman, undefined);
    assert.match(result.output, /git diff failed/);
  });

  it("fails when the check passes over uncommitted changes", async () => {
    const result = await runCheck(
      sandboxWith({
        [diff]: unchanged,
        [check]: { stdout: "ok", exitCode: 0 },
        [status]: { stdout: " M src/Api/Program.cs\n", exitCode: 0 },
      }),
      base,
    );
    assert.equal(result.passed, false);
    assert.match(result.output, /uncommitted changes:\n M src\/Api\/Program\.cs/);
  });

  it("fails when git status fails", async () => {
    const result = await runCheck(
      sandboxWith({
        [diff]: unchanged,
        [check]: { stdout: "ok", exitCode: 0 },
        [status]: { stdout: "fatal: not a git repository", exitCode: 128 },
      }),
      base,
    );
    assert.equal(result.passed, false);
    assert.match(result.output, /git status failed/);
  });
});

describe("headOf", () => {
  it("returns the commit, or undefined when git fails", async () => {
    assert.equal(await headOf(sandboxWith({ "git rev-parse HEAD": { stdout: "abc\n", exitCode: 0 } })), "abc");
    assert.equal(await headOf(sandboxWith({ "git rev-parse HEAD": { stdout: "fatal", exitCode: 128 } })), undefined);
  });
});

describe("shellQuote", () => {
  it("keeps text intact through the shell, quotes and all", () => {
    const text = "it's $HOME `x` \"y\"";
    assert.equal(execFileSync("bash", ["-c", `printf %s ${shellQuote(text)}`], { encoding: "utf8" }), text);
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

// The real commands in a throwaway repo: main has a check.sh, and an issue
// branch cut before it existed doesn't.
describe("the check in a git repo", () => {
  const exec = (cwd: string) => async (command: string): Promise<Result> => {
    try {
      return { stdout: execFileSync("bash", ["-c", command], { cwd, encoding: "utf8" }), stderr: "", exitCode: 0 };
    } catch (error) {
      const failed = error as { stdout?: string; status?: number };
      return { stdout: failed.stdout ?? "", stderr: "", exitCode: failed.status ?? 1 };
    }
  };
  const withRepo = async (checkScript: string, test: (dir: string) => Promise<void>) => {
    const dir = mkdtempSync(join(tmpdir(), "check-test-"));
    const git = (...args: string[]) => execFileSync("git", args, { cwd: dir, stdio: "ignore" });
    try {
      git("init", "-q", "-b", "main");
      git("config", "user.email", "test@example.com");
      git("config", "user.name", "Test");
      writeFileSync(join(dir, "README.md"), "x\n");
      git("add", ".");
      git("commit", "-q", "-m", "first");
      git("branch", "feature/1-old");
      mkdirSync(join(dir, ".sandcastle"));
      writeFileSync(join(dir, ".sandcastle", "check.sh"), checkScript);
      git("add", ".");
      git("commit", "-q", "-m", "add check");
      git("checkout", "-q", "feature/1-old");
      await test(dir);
    } finally {
      rmSync(dir, { recursive: true, force: true });
    }
  };

  it("runs the base branch's check.sh on a branch that has none", () =>
    withRepo("echo base check ran\n", async (dir) => {
      assert.deepEqual(await runCheck({ exec: exec(dir) }, baseCheck("main", dir)), {
        passed: true,
        output: "base check ran\n",
      });
    }));

  it("fails when the base branch's check.sh fails", () =>
    withRepo("echo broken; exit 3\n", async (dir) => {
      assert.deepEqual(await runCheck({ exec: exec(dir) }, baseCheck("main", dir)), { passed: false, output: "broken\n" });
    }));

  it("gives the script no stdin, so a command that reads it can't swallow the rest", () =>
    withRepo("cat >/dev/null\necho still running\nexit 4\n", async (dir) => {
      assert.deepEqual(await runCheck({ exec: exec(dir) }, baseCheck("main", dir)), {
        passed: false,
        output: "still running\n",
      });
    }));

  it("can't resolve a base without a check.sh", () =>
    withRepo("echo ok\n", async (dir) => {
      assert.throws(() => baseCheck("feature/1-old", dir));
    }));
});
