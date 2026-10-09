// Every git command the host runs. Agents can write the clone's shared .git
// (its config, hooks and refs), so the host treats the clone as untrusted:
//
// - Network git (fetch, ls-remote, push) runs only in a host-only bare repo
//   outside the clone, at ~/.cache/sandcastle/<owner>-<name>.git, whose
//   config the sandbox can't reach. Its origin is the GitHub URL from
//   config.mts, never a remote read from the clone's config.
// - Objects move between the two with local fetches. The host repo fetching
//   from the clone runs git-upload-pack there, which git keeps safe to run in
//   an untrusted repository (it runs no hooks, and ignores
//   uploadpack.packObjectsHook from repo config).
// - The few commands the host runs in the clone itself (reading refs and
//   history, and fetching from the host repo) go through cloneGit, which
//   switches off hooks, fsmonitor, alternate-refs commands, replace refs,
//   automatic gc and every transport but local files.
// - The base branch is merged into an issue branch inside the sandbox (see
//   mergeBaseInSandbox in check.mts), where merge drivers and filters can't
//   reach the host.
//
// Sandcastle's own git commands in the clone (creating worktrees, listing
// commits) still read its config; that's outside this repo's control.

import { existsSync, mkdirSync } from "node:fs";
import { homedir } from "node:os";
import { dirname, join } from "node:path";
import { REPO } from "./config.mts";
import { execFileSync } from "node:child_process";
import { sh } from "./shell.mts";

// Settings that override anything the clone's own config sets.
export const untrustedCloneConfig = [
  "--no-replace-objects",
  "-c", "core.hooksPath=/dev/null",
  "-c", "core.fsmonitor=false",
  // Fetch negotiation runs this for every alternate object store an agent
  // lists in objects/info/alternates; `true` lists no refs.
  "-c", "core.alternateRefsCommand=true",
  "-c", "gc.auto=0",
  "-c", "maintenance.auto=false",
  "-c", "protocol.allow=never",
  "-c", "protocol.file.allow=always",
  ...["ext", "fd", "git", "http", "https", "ssh"].flatMap((name) => ["-c", `protocol.${name}.allow=never`]),
];

// Run git in the clone (or one of its worktrees) with untrustedCloneConfig.
export const cloneGit = (cwd: string, ...args: string[]) => sh(cwd, "git", ...untrustedCloneConfig, ...args);

// Run git in the host repo. It never prompts: a missing credential fails the
// command at once instead of waiting on a terminal mid-run.
const hostGit = (host: string, ...args: string[]) =>
  execFileSync("git", args, {
    cwd: host,
    encoding: "utf8",
    stdio: ["ignore", "pipe", "inherit"],
    env: { ...process.env, GIT_TERMINAL_PROMPT: "0" },
  }).trim();

// Fail before any agent runs when git has no HTTPS credential for GitHub,
// which the host repo's pushes need (`gh auth setup-git` provides one).
export function requirePushCredentials(host: string): void {
  try {
    execFileSync("git", ["credential", "fill"], {
      cwd: host,
      encoding: "utf8",
      stdio: ["pipe", "pipe", "ignore"],
      input: "protocol=https\nhost=github.com\n\n",
      env: { ...process.env, GIT_TERMINAL_PROMPT: "0", GIT_ASKPASS: "", SSH_ASKPASS: "" },
    });
  } catch {
    throw new Error("git has no HTTPS credential for github.com, so Sandcastle couldn't push. Run `gh auth setup-git`.");
  }
}

// The host-only bare repo, created on first use.
export function hostRepo(path = join(homedir(), ".cache", "sandcastle", `${REPO.replace("/", "-")}.git`)): string {
  if (!existsSync(path)) {
    mkdirSync(dirname(path), { recursive: true });
    sh(dirname(path), "git", "init", "--quiet", "--bare", path);
    sh(path, "git", "remote", "add", "origin", `https://github.com/${REPO}.git`);
  }
  return path;
}

// The host repo and the clone the sandboxes' worktrees belong to.
export type Repos = { host: string; clone: string };

export const liveRepos = (): Repos => ({ host: hostRepo(), clone: process.cwd() });

// Fetch origin's branch into the host repo, then into the clone's
// refs/remotes/origin/<branch>, and return the commit.
export function fetchFromOrigin(repos: Repos, branch: string): string {
  const ref = `refs/remotes/origin/${branch}`;
  hostGit(repos.host, "fetch", "--quiet", "origin", `+refs/heads/${branch}:${ref}`);
  cloneGit(repos.clone, "fetch", "--quiet", "--no-write-fetch-head", "--no-recurse-submodules", repos.host, `+${ref}:${ref}`);
  return hostGit(repos.host, "rev-parse", "--verify", `${ref}^{commit}`);
}

// Branch names from `git ls-remote --heads` output, without refs/heads/.
export function parseHeads(lsRemote: string): string[] {
  return lsRemote
    .split("\n")
    .filter(Boolean)
    .map((line) => line.split("\t")[1]!.replace(/^refs\/heads\//, ""));
}

// The branches on origin matching the ref patterns, read from the host repo.
export function remoteBranches(repos: Repos, patterns: readonly string[]): string[] {
  return parseHeads(hostGit(repos.host, "ls-remote", "--heads", "origin", ...patterns));
}

// The clone's local branches matching the ref prefixes.
export function localBranches(repos: Repos, prefixes: readonly string[]): string[] {
  return cloneGit(repos.clone, "for-each-ref", "--format=%(refname:short)", ...prefixes).split("\n").filter(Boolean);
}

// Point the clone's local branch at origin's when it's missing or behind, so
// the sandbox starts from the work already pushed. A local branch with
// commits origin lacks is left alone (its push then fails rather than lose
// them), and so is one checked out in a worktree, which Sandcastle reuses
// and fast-forwards itself.
export function syncLocalBranch(repos: Repos, branch: string): void {
  const remote = `refs/remotes/origin/${branch}`;
  const local = `refs/heads/${branch}`;
  let exists = true;
  try {
    cloneGit(repos.clone, "rev-parse", "--verify", "--quiet", local);
  } catch {
    exists = false;
  }
  try {
    if (!exists) {
      cloneGit(repos.clone, "branch", "--no-track", branch, remote);
      return;
    }
    cloneGit(repos.clone, "merge-base", "--is-ancestor", local, remote);
  } catch {
    return;
  }
  try {
    cloneGit(repos.clone, "branch", "--force", "--no-track", branch, remote);
  } catch {
    // Checked out in a worktree.
  }
}

// The commit a worktree has checked out, and the branch it's on ("HEAD" when
// detached).
export function worktreeHead(worktreePath: string): { sha: string; branch: string } {
  return {
    sha: cloneGit(worktreePath, "rev-parse", "--verify", "HEAD^{commit}"),
    branch: cloneGit(worktreePath, "rev-parse", "--abbrev-ref", "HEAD"),
  };
}

// The base is always the commit the host pinned for the round (baseCheck in
// check.mts), never a ref name, since agents can move the clone's refs.

// Count the commits at `sha` that the base doesn't have.
export function commitsAhead(worktreePath: string, baseSha: string, sha: string): number {
  return Number(cloneGit(worktreePath, "rev-list", "--count", `${baseSha}..${sha}`));
}

// Whether `sha` already holds everything on the base.
export function containsBase(worktreePath: string, baseSha: string, sha: string): boolean {
  try {
    cloneGit(worktreePath, "merge-base", "--is-ancestor", baseSha, sha);
    return true;
  } catch {
    return false;
  }
}

// Push exactly the commit the host checked, never whatever the branch points
// at by then: the host repo fetches the branch from the clone, confirms it
// has the commit, and pushes that commit from its own config. Not forced, so
// a branch on origin with commits this one lacks fails the push rather than
// losing them. No hook runs anywhere, so the Docker test suites are left to
// CI.
export function pushChecked(repos: Repos, sha: string, branch: string): void {
  hostGit(repos.host, "fetch", "--quiet", "--no-write-fetch-head", repos.clone, `+refs/heads/${branch}:refs/sandcastle/${branch}`);
  hostGit(repos.host, "cat-file", "-e", `${sha}^{commit}`);
  hostGit(repos.host, "push", "--quiet", "origin", `${sha}:refs/heads/${branch}`);
}
