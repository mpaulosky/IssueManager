// The host's check of an issue branch: .sandcastle/check.sh, run inside the
// sandbox. Its exit code decides whether the branch counts as complete, never
// what an agent says about it.
//
// The agent can edit anything on its branch, and any ref or config in the
// repo its worktree shares, so nothing the check trusts is looked up in the
// sandbox. The host resolves the base branch to a commit, reads that
// commit's check.sh, and passes its text in; the sandbox only runs it. A
// branch that changes the files the script runs from the worktree fails, and
// is left for a human. Running the base's copy also checks branches cut
// before check.sh existed.

import { execFileSync } from "node:child_process";
import type { Sandbox } from "@ai-hero/sandcastle";

export type CheckRun = {
  passed: boolean;
  output: string;
  // The branch changed the check's own files, so a person has to look.
  needsHuman?: boolean;
};

// What the host trusts: a commit of the base branch and its check.sh.
export type BaseCheck = { sha: string; script: string };

// The files check.sh runs from the worktree: package.json holds the
// test:sandcastle script.
export const checkFiles = [
  ".sandcastle/check.sh",
  ".github/scripts/discover_tests.py",
  ".github/ci/gate-checks.sh",
  "package.json",
];

// A shell word that stands for text, whatever it contains.
export function shellQuote(text: string): string {
  return `'${text.replace(/'/g, `'\\''`)}'`;
}

// Resolve the base branch and read its check.sh, on the host. Call it before
// any agent of the round runs, so no agent can move the ref first.
export function baseCheck(baseBranch: string, cwd?: string): BaseCheck {
  const git = (...args: string[]) => execFileSync("git", args, { cwd, encoding: "utf8" });
  const sha = git("rev-parse", "--verify", `${baseBranch}^{commit}`).trim();
  return { sha, script: git("show", `${sha}:.sandcastle/check.sh`) };
}

// The commit the sandbox's worktree is on, or undefined when git fails.
export async function headOf(sandbox: Pick<Sandbox, "exec">): Promise<string | undefined> {
  const head = await sandbox.exec("git rev-parse HEAD");
  return head.exitCode === 0 ? head.stdout.trim() : undefined;
}

// Run the check with stderr folded into stdout, so the output keeps the order
// it was printed in. The script gets its text as an argument and /dev/null as
// stdin, so no command in it can read the rest of the script. A passing check
// over a dirty worktree still fails: only commits are merged, so uncommitted
// edits would be checked but never land.
export async function runCheck(sandbox: Pick<Sandbox, "exec">, base: BaseCheck): Promise<CheckRun> {
  // What the branch changed since it left the base (three dots), so a base
  // that moved on since doesn't count against it.
  const changed = await sandbox.exec(`git diff --name-only ${base.sha}...HEAD -- ${checkFiles.join(" ")} 2>&1`);
  if (changed.exitCode !== 0) {
    return { passed: false, output: `git diff failed, so the check's own files can't be shown unchanged:\n${changed.stdout}` };
  }
  if (changed.stdout.trim()) {
    return {
      passed: false,
      needsHuman: true,
      output: `The branch changes the check's own files, which a human must review:\n${changed.stdout}`,
    };
  }

  const { stdout, exitCode } = await sandbox.exec(`bash -c ${shellQuote(base.script)} check.sh </dev/null 2>&1`);
  if (exitCode !== 0) return { passed: false, output: stdout };

  const status = await sandbox.exec("git status --porcelain 2>&1");
  if (status.exitCode !== 0) {
    return { passed: false, output: `${stdout}\ngit status failed, so the worktree can't be shown to be clean:\n${status.stdout}` };
  }
  if (status.stdout.trim()) {
    return { passed: false, output: `${stdout}\nThe check passed, but the worktree has uncommitted changes:\n${status.stdout}` };
  }
  return { passed: true, output: stdout };
}

// The last `lines` lines of the output.
export function tail(output: string, lines: number): string {
  return output.replace(/\n$/, "").split("\n").slice(-lines).join("\n");
}
