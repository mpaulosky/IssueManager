# Process Violation Remediation — 2026-02-23

## Summary

Gandalf remediated a direct-push process violation by reverting the problematic commit from origin/main, creating a proper feature branch (`feature/release-notes-automation`), running build verification (130 tests passed), and opening PR #40 for review.

## Actions Taken

- Reverted release notes changes from main
- Created `feature/release-notes-automation` branch
- Verified build and tests (130 tests passed)
- Opened PR #40
- Created `.squad/skills/build-repair/SKILL.md` documenting the enforced branching policy

## Decisions Recorded

- Merged inbox decisions into `.squad/decisions/decisions.md`
- Unified branching process enforcement: ALL changes require feature branch + build-repair validation
- Documented that direct pushes to main are now prohibited

## Status

✅ Complete. Branching process is now enforced across all team members.
