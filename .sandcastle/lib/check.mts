// The host's check of an issue branch: .sandcastle/check.sh, run inside the
// sandbox. Its exit code decides whether the branch counts as complete, never
// what an agent says about it.

import type { Sandbox } from "@ai-hero/sandcastle";

export type CheckRun = { passed: boolean; output: string };

// Run the check with stderr folded into stdout, so the output keeps the order
// it was printed in. A passing check over a dirty worktree still fails: only
// commits are merged, so uncommitted edits would be checked but never land.
export async function runCheck(sandbox: Pick<Sandbox, "exec">): Promise<CheckRun> {
  const { stdout, exitCode } = await sandbox.exec(".sandcastle/check.sh 2>&1");
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
