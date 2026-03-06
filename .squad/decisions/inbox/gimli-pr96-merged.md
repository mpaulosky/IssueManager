### 2026-03-07: PR #96 merged — Integration.Tests renamed to Api.Tests.Integration

**What:** Resolved merge conflicts (IssueManager.sln, squad-test.yml, pre-push hook) then squash-merged PR #96.

**Decision:** When a PR branch diverges from main after a squash-merge, use `git merge origin/main` (not rebase) to preserve .gitattributes merge=union behavior for .squad/ files.
