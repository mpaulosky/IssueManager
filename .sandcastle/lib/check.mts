// The host's check of an issue branch: .sandcastle/check.sh, run inside the
// sandbox. Its exit code decides whether the branch counts as complete, not
// what an agent says about it.
//
// The host resolves the base branch to a commit, reads that commit's
// check.sh, and passes its text in; the sandbox only runs it. A branch that
// changes the files the script runs from the worktree fails, and is left for
// a human. Running the base's copy also checks branches cut before check.sh
// existed. The commands run with a fixed PATH, so a shim in the agent's
// ~/.local/bin isn't picked up.
//
// This guards against mistakes and false claims of completion, not against a
// determined agent: the sandbox is the agent's, and the branch's other files
// (a csproj that adds Testcontainers, say, or .npmrc) still steer the check.
// The boundary is CI, which runs everything on the pull request.

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

// The PATH every check command runs with: the image's system directories,
// without the agent-writable ~/.local/bin.
export const fixedPath = "/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin";
const run = `env PATH=${fixedPath}`;

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
  const head = await sandbox.exec(`${run} git rev-parse HEAD`);
  return head.exitCode === 0 ? head.stdout.trim() : undefined;
}

// Merge the base into the sandbox's branch when it's behind, so the work is
// built, checked and reviewed against current main and its PR can merge.
// Runs in the sandbox, where merge drivers and filters from the agent-writable
// config can't reach the host. Returns false, with the merge undone, when it
// conflicts; the implementer is then asked to merge it.
export async function mergeBaseInSandbox(sandbox: Pick<Sandbox, "exec">, baseSha: string): Promise<boolean> {
  if ((await sandbox.exec(`${run} git merge-base --is-ancestor ${baseSha} HEAD`)).exitCode === 0) return true;
  if ((await sandbox.exec(`${run} git merge --no-edit ${baseSha} 2>&1`)).exitCode === 0) return true;
  await sandbox.exec(`${run} git merge --abort 2>&1`);
  return false;
}

// Run the check with stderr folded into stdout, so the output keeps the order
// it was printed in. The script gets its text as an argument and /dev/null as
// stdin, so no command in it can read the rest of the script. A passing check
// over a dirty worktree still fails: only commits are published, so
// uncommitted edits would be checked but never land.
export async function runCheck(sandbox: Pick<Sandbox, "exec">, base: BaseCheck): Promise<CheckRun> {
  // What the branch changed since it left the base (three dots), so a base
  // that moved on since doesn't count against it.
  // Only stdout lists files; a warning on stderr (say, multiple merge bases)
  // mustn't read as one.
  const changed = await sandbox.exec(`${run} git diff --name-only ${base.sha}...HEAD -- ${checkFiles.join(" ")}`);
  if (changed.exitCode !== 0) {
    return { passed: false, output: `git diff failed, so the check's own files can't be shown unchanged:\n${changed.stderr}` };
  }
  if (changed.stdout.trim()) {
    return {
      passed: false,
      needsHuman: true,
      output: `The branch changes the check's own files, which a human must review:\n${changed.stdout}`,
    };
  }

  const { stdout, exitCode } = await sandbox.exec(`${run} bash -c ${shellQuote(base.script)} check.sh </dev/null 2>&1`);
  if (exitCode !== 0) return { passed: false, output: stdout };

  // pnpm leaves its store at the worktree's root in the sandbox; a branch
  // cut before .gitignore listed it would otherwise never pass.
  const status = await sandbox.exec(`${run} git status --porcelain -- . ':(exclude).pnpm-store'`);
  if (status.exitCode !== 0) {
    return { passed: false, output: `${stdout}\ngit status failed, so the worktree can't be shown to be clean:\n${status.stderr}` };
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
