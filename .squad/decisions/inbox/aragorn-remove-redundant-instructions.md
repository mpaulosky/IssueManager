### 2026-03-03: Remove redundant .github/instructions files
**By:** Matthew (confirmed) + Aragorn (executed)
**What:** Removed copilot-sdk-csharp.instructions.md and mongo-dba.instructions.md from .github/instructions/. Both fully covered by squad skills. Net context savings: ~17 KB removed from VS Code Copilot context on every .cs file interaction.
**Why:** Reduce context overhead. Squad skills are on-demand; instruction files load globally.
**Kept:** blazor.instructions.md, markdown.instructions.md, git-commit-instructions.md, copilot-instructions.md — all targeted or small enough to justify.
