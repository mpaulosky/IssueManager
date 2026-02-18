### 2026-02-18: Always sync local main after merge

**By:** mpaulosky (via Copilot)

**What:** After any PR merges into main, the team ensures local main is synced with origin/main before starting the next task.

**Why:** Clean branch state for each new task — prevents stale main and merge conflicts on the next feature branch.

**Implementation:** After Scribe merges a PR, run:
```
git checkout main
git pull origin main
```

Before spawning agents for the next task, verify: `git status` shows main is up-to-date with origin.
