// Branch naming and fetching. Branch names come from code, never from a model,
// so re-planning an issue always lands on the branch that holds its earlier
// work, and every name passes scripts/check-branch-name.sh.

import { fetchFromOrigin, liveRepos, localBranches, remoteBranches, syncLocalBranch } from "./git.mts";

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

// The git operations prepareBranches needs; tests pass a stub.
export type IssueBranchGit = {
  // The issue branches on origin.
  remoteBranches(): string[];
  // The issue branches in this clone, where unpublished work stays.
  localBranches(): string[];
  // Bring origin's branch into the clone, and its local branch up to date.
  fetch(branch: string): void;
};

const liveGit = (): IssueBranchGit => {
  const repos = liveRepos();
  return {
    remoteBranches: () => remoteBranches(repos, issuePrefixes.map((prefix) => `refs/heads/${prefix}/*`)),
    localBranches: () => localBranches(repos, issuePrefixes.map((prefix) => `refs/heads/${prefix}/`)),
    fetch: (branch) => {
      fetchFromOrigin(repos, branch);
      syncLocalBranch(repos, branch);
    },
  };
};

// Name each issue's branch from the issue branches on origin and in this
// clone, and fetch the ones that exist on origin, so the sandbox starts from
// the work already pushed rather than from main.
export function prepareBranches<T extends BranchIssue>(
  issues: readonly T[],
  git: IssueBranchGit = liveGit(),
): { issue: T; branch: string }[] {
  const remote = git.remoteBranches();
  const existing = [...new Set([...remote, ...git.localBranches()])];
  return issues.map((issue) => {
    const branch = branchFor(issue, existing);
    if (remote.includes(branch)) git.fetch(branch);
    return { issue, branch };
  });
}
