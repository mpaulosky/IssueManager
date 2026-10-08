import { execFileSync } from "node:child_process";

// Run a command on the host in `cwd` and return trimmed stdout. Throws on failure.
export const sh = (cwd: string, cmd: string, ...args: string[]) =>
  execFileSync(cmd, args, { cwd, encoding: "utf8", stdio: ["ignore", "pipe", "inherit"] }).trim();

// Settings for every git command the host runs in an issue's worktree. The
// agents can edit the worktree, hooks included, so the host never runs its
// hooks: they would run agent-written code on the host. The Docker test suites
// the pre-push hook would add are left to CI. fsmonitor is off for the same
// reason, since it names a program for git to run.
export const untrustedGitConfig = ["-c", "core.hooksPath=/dev/null", "-c", "core.fsmonitor=false"];

// Run git on the host in an issue's worktree, with untrustedGitConfig.
export const worktreeGit = (worktreePath: string, ...args: string[]) =>
  sh(worktreePath, "git", ...untrustedGitConfig, ...args);
