# Decision: docs/ Folder Convention for .md, .txt, .log Files

**Date:** 2026-02-23
**Requested by:** User (Ralph session)
**Status:** Accepted

## Decision

All `.md`, `.txt`, and `.log` files created during development must be placed in the `docs/` folder
(or a sub-folder such as `docs/guides/`, `docs/reviews/`) — not in the repository root.

## Rationale

Accumulated build artifacts and working documents at the repository root create clutter and confusion.
Centralising them in `docs/` keeps the root clean and makes it easier to find reference material.

## Rules

- **Generated output** (build logs, test results, restore logs): Never committed; covered by `.gitignore`
  (`*.log`, `*.txt`).
- **Working documents** (plans, summaries, instructions, analysis): Place in `docs/` or `docs/guides/`.
- **Standard root files** (README.md, SECURITY.md, LICENSE): Remain at root per GitHub/OSS convention.

## Actions Taken

- Added `*.txt` to `.gitignore` (alongside existing `*.log` rule).
- Moved `PR_CREATION_INSTRUCTIONS.md`, `QUICK_ACTION_PLAN.md`, `WORKFLOW_FIXES_SUMMARY.md` → `docs/`.
- Removed committed build artifact `.txt` files from tracking.
- Added "File Placement" section to `.github/copilot-instructions.md`.
