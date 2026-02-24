### 2026-02-24T18:38:31Z: Branch protection directive

**By:** Matthew Paulosky (via Copilot)

**What:** Main branch is protected. All work must be done on feature/bug branches. Never commit directly to main.

**Why:** Enforcing protected branch workflow — ensures code review gates, CI checks, and approval requirements before any changes reach main.

**Scope:** All agents, all work. Apply to every task that involves code changes or `.squad/` state modifications.

**Enforcement:** Agents should automatically create `squad/{issue-number}-{slug}` or `feature/{slug}` branches, commit there, push, and open PRs. Scribe should enforce this in commit checks before pushing to origin.
