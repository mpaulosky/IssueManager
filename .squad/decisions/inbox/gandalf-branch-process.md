### 2026-02-23: Branching process enforced

**By:** Gandalf (Lead)
**What:** ALL changes must go through a feature branch + build-repair skill before pushing. Direct pushes to main are prohibited. Build-repair skill created at `.squad/skills/build-repair/SKILL.md`.
**Why:** Process violation — Aragorn pushed release notes changes directly to main. Remediated by reverting main, creating feature branch, running build, opening PR.
