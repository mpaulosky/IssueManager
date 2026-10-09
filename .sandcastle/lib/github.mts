// GitHub access. Only the host talks to GitHub: the sandbox gets no token, so
// everything an agent needs from GitHub reaches it through its prompt, and
// every write (comments, pushes, pull requests) is made here, in code.

import { execFileSync } from "node:child_process";
import { sh } from "./shell.mts";

// The author associations whose text may reach an agent. Anyone else can open
// or comment on a public issue, so their words could steer an agent.
export const TRUSTED_ASSOCIATIONS: ReadonlySet<string> = new Set(["OWNER", "MEMBER", "COLLABORATOR"]);

export type RawIssue = {
  number: number;
  title: string;
  body: string;
  authorAssociation: string;
  labels: string[];
  comments: { authorAssociation: string; body: string }[];
};

export type SandcastleIssue = {
  number: number;
  title: string;
  body: string;
  labels: string[];
  // Only comments from trusted authors; see trustedIssues.
  comments: string[];
};

// Keep only what trusted authors wrote: an issue opened by anyone else is
// dropped (its title and body can't be trusted), and so is every untrusted
// comment. Returns the kept issues and the numbers of the dropped ones.
export function trustedIssues(raw: readonly RawIssue[]): { issues: SandcastleIssue[]; untrusted: number[] } {
  const issues: SandcastleIssue[] = [];
  const untrusted: number[] = [];
  for (const issue of raw) {
    if (!TRUSTED_ASSOCIATIONS.has(issue.authorAssociation)) {
      untrusted.push(issue.number);
      continue;
    }
    issues.push({
      number: issue.number,
      title: issue.title,
      body: issue.body,
      labels: issue.labels,
      comments: issue.comments
        .filter((comment) => TRUSTED_ASSOCIATIONS.has(comment.authorAssociation))
        .map((comment) => comment.body),
    });
  }
  return { issues, untrusted };
}

let repo: { owner: string; name: string } | undefined;

// The repository in the current directory. Read on first use rather than at
// import, so importing a module never shells out to gh.
export function repoName(): { owner: string; name: string } {
  if (!repo) {
    const [owner, name] = sh(process.cwd(), "gh", "repo", "view", "--json", "nameWithOwner", "--jq", ".nameWithOwner").split("/");
    repo = { owner: owner!, name: name! };
  }
  return repo;
}

const issuesQuery = `
query($owner: String!, $name: String!) {
  repository(owner: $owner, name: $name) {
    issues(states: OPEN, labels: ["Sandcastle"], first: 100) {
      pageInfo { hasNextPage }
      nodes {
        number title body authorAssociation
        labels(first: 50) { nodes { name } }
        comments(last: 100) { nodes { authorAssociation body } }
      }
    }
  }
}`;

type IssuesResponse = {
  data: {
    repository: {
      issues: {
        pageInfo: { hasNextPage: boolean };
        nodes: {
          number: number;
          title: string;
          body: string;
          authorAssociation: string;
          labels: { nodes: { name: string }[] };
          comments: { nodes: { authorAssociation: string; body: string }[] };
        }[];
      };
    };
  };
};

// The label that hands an issue to a person. Sandcastle adds it when a branch
// changes the check's own files; the owner removing it re-queues the issue.
export const NEEDS_HUMAN = "sandcastle:needs-human";

// Whether Sandcastle may work an issue: not while it's handed to a person.
export function isQueued(issue: Pick<SandcastleIssue, "labels">): boolean {
  return !issue.labels.includes(NEEDS_HUMAN);
}

// The open issues labelled Sandcastle and not handed to a person, keeping only
// what trusted authors wrote.
export function listSandcastleIssues(): SandcastleIssue[] {
  const { owner, name } = repoName();
  const response = JSON.parse(
    sh(process.cwd(), "gh", "api", "graphql", "-f", `query=${issuesQuery}`, "-F", `owner=${owner}`, "-F", `name=${name}`),
  ) as IssuesResponse;
  const { pageInfo, nodes } = response.data.repository.issues;
  if (pageInfo.hasNextPage) console.warn("  More than 100 open Sandcastle issues: only the first 100 are considered.");

  const { issues, untrusted } = trustedIssues(
    nodes.map((issue) => ({
      number: issue.number,
      title: issue.title,
      body: issue.body,
      authorAssociation: issue.authorAssociation,
      labels: issue.labels.nodes.map((label) => label.name),
      comments: issue.comments.nodes,
    })),
  );
  for (const number of untrusted) {
    console.warn(`  Skipping #${number}: its author isn't an owner, member or collaborator. Re-file it to have it built.`);
  }
  return issues.filter(isQueued);
}

export type PullRequestHead = { headRefName: string; isCrossRepository: boolean; url: string };

// The pull requests opened from this repository's own branches. Anyone can
// open a PR from a fork, and name its branch after an issue's, so a fork PR
// must neither hold an issue back nor pass for the issue's own PR.
export function sameRepoPullRequests<T extends Pick<PullRequestHead, "isCrossRepository">>(prs: readonly T[]): T[] {
  return prs.filter((pr) => !pr.isCrossRepository);
}

const pullRequestFields = ["headRefName", "isCrossRepository", "url"].join(",");

// The head branches of the open same-repo pull requests.
export function openPullRequestBranches(): string[] {
  const prs = JSON.parse(
    sh(process.cwd(), "gh", "pr", "list", "--state", "open", "--limit", "1000", "--json", pullRequestFields),
  ) as PullRequestHead[];
  return sameRepoPullRequests(prs).map((pr) => pr.headRefName);
}

export function commentOnIssue(issue: number, body: string): void {
  execFileSync("gh", ["issue", "comment", String(issue), "--body-file", "-"], {
    cwd: process.cwd(),
    encoding: "utf8",
    stdio: ["pipe", "pipe", "inherit"],
    input: body,
  });
}

export function labelIssue(issue: number, label: string): void {
  execFileSync("gh", ["issue", "edit", String(issue), "--add-label", label], { stdio: ["ignore", "ignore", "inherit"] });
}

// Open a draft pull request for the branch, or return the open same-repo one
// it already has. A draft is never merged, so a person reviews the agents'
// work and marks it ready.
export function openPullRequest(branch: string, title: string, body: string): string {
  const existing = sameRepoPullRequests(
    JSON.parse(
      sh(process.cwd(), "gh", "pr", "list", "--head", branch, "--state", "open", "--json", pullRequestFields),
    ) as PullRequestHead[],
  )[0];
  if (existing) return existing.url;
  return execFileSync(
    "gh",
    ["pr", "create", "--draft", "--base", "main", "--head", branch, "--title", title, "--body-file", "-"],
    { cwd: process.cwd(), encoding: "utf8", stdio: ["pipe", "pipe", "inherit"], input: body },
  ).trim();
}
