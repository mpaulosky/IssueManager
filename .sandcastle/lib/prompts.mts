// Prompt arguments built from GitHub content. Agents can't reach GitHub, so
// this is everything they learn about an issue. The issues passed in hold
// only what trusted authors wrote (see trustedIssues).
//
// Sandcastle runs a prompt's !`command` blocks after filling in {{KEY}}, but
// marks the blocks the prompt file itself holds first and runs only those, so
// a !`...` inside an issue's title or body is passed on as text, never run
// (checked against @ai-hero/sandcastle 0.12.0's substitutePromptArgs). An
// argument filled in inside a !`...` block would be run, so only the
// host-made BRANCH and BASE_BRANCH go there.

import type { SandcastleIssue } from "./github.mts";

// Sandcastle sets {{TARGET_BRANCH}} itself (to the sandbox's own branch inside
// createSandbox) and refuses an override, so the base to compare against goes
// in as {{BASE_BRANCH}}: the commit of origin/main the host pinned for the
// round, which no agent can move.
export function issuePromptArgs(issue: SandcastleIssue, branch: string, baseSha: string) {
  return {
    TASK_ID: String(issue.number),
    ISSUE_TITLE: issue.title,
    ISSUE_BODY: issue.body || "(no description)",
    ISSUE_COMMENTS: issue.comments.length > 0 ? issue.comments.join("\n\n---\n\n") : "(no comments)",
    BRANCH: branch,
    BASE_BRANCH: baseSha,
  };
}

export function plannerPromptArgs(ready: readonly SandcastleIssue[]) {
  return { ISSUES_JSON: JSON.stringify(ready) };
}
