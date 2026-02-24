### 2026-02-24T202812Z: PR merge and cleanup workflow directive

**By:** Matthew Paulosky (via Copilot)

**What:** When a PR is created and in work, resolve any issues (test failures, merge conflicts, etc.). When the PR is ready to merge, execute the merge immediately, then clean up the local feature branch and pull origin main into local main to sync.

**Why:** User request — automated workflow for PR lifecycle management. Ensures PRs don't linger, reduces merge debt, and keeps main branch synchronized across local and origin.

**Implementation workflow:**
1. When spawning agents to create/work on PRs, include: "If the PR checks pass and there are no blocking comments, merge via `gh pr merge {number} --squash --delete-branch`."
2. After merge completes, run:
   - `git checkout main` (switch to main)
   - `git pull origin main` (sync with latest)
   - Feature branch is auto-deleted by gh pr merge
3. If PR has failing checks or requested changes, spawn appropriate agent to fix before merge.

**Applies to:** All agents creating PRs or addressing PR feedback. Aragorn (Lead) is responsible for final merge decision.
