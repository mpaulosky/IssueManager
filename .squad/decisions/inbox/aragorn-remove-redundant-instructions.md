---
post_title: Remove redundant .github/instructions files
author1: Matthew (confirmed) and Aragorn (executed)
post_slug: aragorn-remove-redundant-instructions
microsoft_alias: matthew;aragorn
featured_image: /images/decisions/default.png
categories:
  - Architecture
tags:
  - decision
  - copilot
  - instructions
ai_note: Content reviewed and approved by human authors.
summary: >
  Decision to remove redundant .github/instructions files now fully covered
  by squad skills, reducing Copilot context size for C# interactions.
post_date: 2026-03-03
---

## Remove redundant .github/instructions files

**Date:** 2026-03-03
**Author:** Matthew (confirmed) + Aragorn (executed)
**Status:** Completed

**What:** Removed copilot-sdk-csharp.instructions.md and
mongo-dba.instructions.md from `.github/instructions/`. Both are fully
covered by squad skills. Net context savings: ~17 KB removed from VS Code
Copilot context on every `.cs` file interaction.

**Why:** Reduce context overhead. Squad skills are on-demand; instruction
files load globally.

**Kept:** blazor.instructions.md, markdown.instructions.md,
git-commit-instructions.md, copilot-instructions.md — all targeted or
small enough to justify.

