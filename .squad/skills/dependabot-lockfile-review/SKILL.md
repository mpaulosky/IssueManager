---
name: dependabot-lockfile-review
description: >
  Review and validate lockfile-only Dependabot PRs for IssueManager frontend
  dependencies before merging.
domain: dependency-management
confidence: high
source: earned
tools:
  - name: github-mcp-server-pull_request_read
    description: Read PR metadata, changed files, reviews, and check runs.
    when: Use first to confirm scope and CI state.
  - name: gh
    description: Approve and merge the Dependabot PR after validation.
    when: Use once the update is confirmed safe.
---

## Context

Use this skill for Dependabot PRs that only change `src/Web/package-lock.json`
or other frontend lockfiles. These PRs are usually low-risk, but the lockfile
can contain more churn than the single advertised package bump.

## Patterns

- Confirm the PR scope is lockfile-only and does not modify `package.json`.
- Check that the bumped package version matches the intended Dependabot update.
- Review CI and make sure required checks are green.
- Treat cancelled non-required jobs as non-blocking when the PR still reports
  green overall status.
- Validate the branch in an isolated worktree with
  `npm ci --ignore-scripts` from `src/Web/`.
- Accept nested optional wasm helper additions in the lockfile when install
  validation succeeds and the targeted dependency resolves to the expected
  version.

## Examples

- PR #113 updated `picomatch` from `4.0.3` to `4.0.4` in
  `src/Web/package-lock.json`.
- Validation used:

```bash
git worktree add --detach .copilot-worktrees/pr113 origin/main
git -C .copilot-worktrees/pr113 fetch --quiet origin pull/113/head:pr-113
git -C .copilot-worktrees/pr113 checkout --quiet pr-113
cd .copilot-worktrees/pr113/src/Web
npm ci --ignore-scripts --quiet
```

## Anti-Patterns

- Do not reject a PR only because `package-lock.json` adds extra nested package
  entries when the install is still clean.
- Do not rely only on the PR title; inspect the resolved lockfile contents.
- Do not merge before confirming the PR is still against the expected head
  commit.
