### 2026-02-24T205611Z: Branch cleanup routine

**By:** Copilot (via user request)

**What:** Establish routine cleanup of orphaned local branches. Main branch must never be removed.

**Why:** Maintain clean local workspace; prevent accumulation of stale squad/* branches after PRs merge.

**Implementation:** Create `.squad/skills/branch-cleanup/SKILL.md` with safe cleanup patterns (fetch-prune, delete merged branches, whitelist main/develop).

---
