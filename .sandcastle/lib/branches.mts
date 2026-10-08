// Branch naming and fetching. Branch names come from code, never from a model,
// so re-planning an issue always lands on the branch that holds its earlier
// work, and every name passes scripts/check-branch-name.sh.

import { BASE_BRANCH } from "./config.mts";
import { sh, worktreeGit } from "./shell.mts";

const maxSlugLength = 50;

// The issue-branch prefixes from docs/PROCESS.md. Sandcastle creates feature/
// and fix/ branches; a hotfix/ branch someone made by hand is still reused.
const issuePrefixes = ["feature", "fix", "hotfix"] as const;

// The parts of an issue its branch name depends on.
export type BranchIssue = { number: number; title: string; labels: string[] };

// The issue title as a branch slug: no conventional-commit prefix, lower case,
// apostrophes dropped so "haven't" stays one word, and runs of ASCII letters
// and digits joined with "-", cut to 50 characters at a "-" boundary.
export function slugFor(title: string): string {
  const words = title
    .replace(/^\s*[a-z]+(?:\([^)]*\))?!?:\s*/i, "")
    .toLowerCase()
    .replace(/['’]/g, "")
    .match(/[a-z0-9]+/g) ?? [];
  const slug = words.join("-");
  if (slug.length === 0) return "issue";
  if (slug.length <= maxSlugLength) return slug;

  const cut = slug.slice(0, maxSlugLength + 1);
  const boundary = cut.lastIndexOf("-");
  return boundary > 0 ? cut.slice(0, boundary) : slug.slice(0, maxSlugLength);
}

// Whether a branch belongs to the issue: feature/{n}-*, fix/{n}-* or hotfix/{n}-*.
export function isIssueBranch(branch: string, issueNumber: number): boolean {
  return issuePrefixes.some((prefix) => branch.startsWith(`${prefix}/${issueNumber}-`));
}

// The issue's branch: its existing feature/, fix/ or hotfix/{n}-* branch when
// there is one, even if the title or labels have changed since, so earlier
// work is built on rather than redone. Otherwise fix/{n}-{slug} for a bug and
// feature/{n}-{slug} for everything else.
export function branchFor(issue: BranchIssue, existingBranches: readonly string[]): string {
  const existing = existingBranches
    .filter((branch) => isIssueBranch(branch, issue.number))
    .sort()[0];
  if (existing) return existing;

  const prefix = issue.labels.includes("bug") ? "fix" : "feature";
  return `${prefix}/${issue.number}-${slugFor(issue.title)}`;
}

// Hold back every issue that already has an open pull request from one of its
// branches: its work is waiting for review, and building it again would only
// pile commits onto that PR.
export function withoutOpenPullRequests<T extends BranchIssue>(
  issues: readonly T[],
  openPrBranches: readonly string[],
): { ready: T[]; inReview: T[] } {
  const ready: T[] = [];
  const inReview: T[] = [];
  for (const issue of issues) {
    (openPrBranches.some((branch) => isIssueBranch(branch, issue.number)) ? inReview : ready).push(issue);
  }
  return { ready, inReview };
}

// Each issue once, in first-seen order. A plan that names an issue twice
// would otherwise start two pipelines on the same branch.
export function uniqueIssues<T extends { number: number }>(issues: readonly T[]): T[] {
  const seen = new Set<number>();
  return issues.filter((issue) => !seen.has(issue.number) && seen.add(issue.number));
}

// Branch names from `git ls-remote --heads` output, without refs/heads/.
export function parseHeads(lsRemote: string): string[] {
  return lsRemote
    .split("\n")
    .filter(Boolean)
    .map((line) => line.split("\t")[1]!.replace(/^refs\/heads\//, ""));
}

// The git operations prepareBranches needs; tests pass a stub.
export type IssueBranchGit = {
  // The issue branches on origin.
  remoteBranches(): string[];
  // The issue branches in this clone, where unpublished work stays.
  localBranches(): string[];
  // Fetch origin's branch into its remote-tracking ref.
  fetch(branch: string): void;
};

const cloneGit: IssueBranchGit = {
  remoteBranches: () =>
    parseHeads(
      sh(process.cwd(), "git", "ls-remote", "--heads", "origin", ...issuePrefixes.map((prefix) => `refs/heads/${prefix}/*`)),
    ),
  localBranches: () =>
    sh(process.cwd(), "git", "for-each-ref", "--format=%(refname:short)", ...issuePrefixes.map((prefix) => `refs/heads/${prefix}/`))
      .split("\n")
      .filter(Boolean),
  fetch: (branch) => {
    sh(process.cwd(), "git", "fetch", "--quiet", "origin", `+refs/heads/${branch}:refs/remotes/origin/${branch}`);
  },
};

// Refresh origin/main, the base of every new issue branch and of every diff
// the reviewer reads. Done once per round, before the pipelines start, because
// concurrent fetches would contend on the same ref lock.
export function fetchMain(): void {
  sh(process.cwd(), "git", "fetch", "--quiet", "origin", "main");
}

// Name each issue's branch from the issue branches on origin and in this
// clone, and fetch the ones that exist on origin, so the sandbox starts from
// the work already pushed rather than from main.
export function prepareBranches<T extends BranchIssue>(
  issues: readonly T[],
  git: IssueBranchGit = cloneGit,
): { issue: T; branch: string }[] {
  const remote = git.remoteBranches();
  const existing = [...new Set([...remote, ...git.localBranches()])];
  return issues.map((issue) => {
    const branch = branchFor(issue, existing);
    if (remote.includes(branch)) git.fetch(branch);
    return { issue, branch };
  });
}

// The commit the worktree has checked out, and the branch it's on ("HEAD"
// when detached).
export function headOf(worktreePath: string): { sha: string; branch: string } {
  return {
    sha: worktreeGit(worktreePath, "rev-parse", "HEAD"),
    branch: worktreeGit(worktreePath, "rev-parse", "--abbrev-ref", "HEAD"),
  };
}

// Count the commits at `sha` that the base branch doesn't have.
export function commitsAhead(worktreePath: string, sha: string): number {
  return Number(worktreeGit(worktreePath, "rev-list", "--count", `${BASE_BRANCH}..${sha}`));
}

// Whether `sha` already holds everything on the base branch.
export function containsBase(worktreePath: string, sha: string): boolean {
  try {
    worktreeGit(worktreePath, "merge-base", "--is-ancestor", BASE_BRANCH, sha);
    return true;
  } catch {
    return false;
  }
}

// Merge the base branch into the worktree's branch when it's behind, so the
// work is built, checked and reviewed against current main and its PR can
// merge. Returns false, with the merge undone, when it conflicts; the
// implementer is then asked to merge it.
export function mergeBase(worktreePath: string): boolean {
  if (containsBase(worktreePath, "HEAD")) return true;
  try {
    worktreeGit(worktreePath, "merge", "--no-edit", BASE_BRANCH);
    return true;
  } catch {
    try {
      worktreeGit(worktreePath, "merge", "--abort");
    } catch {
      // Nothing to abort: the merge failed before it started.
    }
    return false;
  }
}

// Push exactly the commit the host checked, never whatever the branch points
// at by then. Not forced: a branch on origin that has commits this one lacks
// fails the push rather than losing them. No hooks run (see worktreeGit), so
// the pre-push gate's Docker test suites are left to CI.
export function pushChecked(worktreePath: string, sha: string, branch: string): void {
  worktreeGit(worktreePath, "push", "--quiet", "origin", `${sha}:refs/heads/${branch}`);
}
