// The host's check of an issue branch: .sandcastle/check.sh, run inside the
// sandbox. Its exit code decides whether the branch counts as complete, never
// what an agent says about it.
//
// The agent can edit anything on its branch, check.sh included, so the host
// runs the base branch's copy of check.sh, and fails a branch that changes
// the scripts the check calls; a human reviews those changes. Running the
// base's copy also checks branches cut before check.sh existed.

import type { Sandbox } from "@ai-hero/sandcastle";

export type CheckRun = { passed: boolean; output: string };

// The check script and the scripts it runs from the worktree.
export const checkFiles = [
  ".sandcastle/check.sh",
  ".github/scripts/discover_tests.py",
  ".github/ci/gate-checks.sh",
];

// A shell word that stands for text, whatever it contains.
export function shellQuote(text: string): string {
  return `'${text.replace(/'/g, `'\\''`)}'`;
}

// Run the check with stderr folded into stdout, so the output keeps the order
// it was printed in. A passing check over a dirty worktree still fails: only
// commits are merged, so uncommitted edits would be checked but never land.
export async function runCheck(sandbox: Pick<Sandbox, "exec">, baseBranch: string): Promise<CheckRun> {
  const base = shellQuote(baseBranch);

  // What the branch changed since it left the base (three dots), so a base
  // that moved on since doesn't count against it.
  const changed = await sandbox.exec(`git diff --name-only ${base}...HEAD -- ${checkFiles.join(" ")} 2>&1`);
  if (changed.exitCode !== 0) {
    return { passed: false, output: `git diff failed, so the check's own files can't be shown unchanged:\n${changed.stdout}` };
  }
  if (changed.stdout.trim()) {
    return {
      passed: false,
      output: `The branch changes the check's own files, which a human must review:\n${changed.stdout}`,
    };
  }

  // pipefail: a git show that fails must fail the check, not feed bash nothing.
  const script = `set -o pipefail; git show ${base}:.sandcastle/check.sh | bash -s`;
  const { stdout, exitCode } = await sandbox.exec(`bash -c ${shellQuote(script)} 2>&1`);
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

// Text quoted in a fence longer than any run of backticks inside it.
export function fenced(text: string): string {
  const longestRun = Math.max(0, ...[...text.matchAll(/`+/g)].map((match) => match[0].length));
  const fence = "`".repeat(Math.max(3, longestRun + 1));
  return `${fence}text\n${text}\n${fence}`;
}
