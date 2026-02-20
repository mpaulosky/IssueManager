### 2026-02-19: CRITICAL DIRECTIVE — Never work on main

**By:** User (via Copilot)

**What:** "We should not be working on main we should never work on main! We always should ensure main is clean then create a feature branch to work from."

**Why:** User directive — enforcing Git workflow discipline. Main branch must ALWAYS remain clean and protected. ALL work happens on feature branches only.

**Implementation:**
1. Before starting ANY work: Verify main is clean and synced with origin/main
2. Create feature branch for each work sprint: `git checkout -b squad/{work-area}`
3. Commit and push ONLY to feature branch
4. Create PR for review + merge when work completes
5. Lead approves and merges PR to main
6. Main never receives direct commits from agents

**Scope:** Global — applies to ALL future work on this repo.

**Status:** ACTIVE — all agents must follow this going forward.
