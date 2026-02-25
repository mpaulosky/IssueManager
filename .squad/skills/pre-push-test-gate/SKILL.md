---
name: pre-push-test-gate
confidence: high
description: >
  Enforces local test execution before any git push by installing a pre-push hook.
  Prevents agents and developers from pushing failing tests to the remote.
  Established after the Shared project test batch (04714a4) shipped two broken tests.
---

## Pre-Push Test Gate

### Why This Exists

On 2026-02-25, two unit tests were pushed directly to `main` without local verification.
Both tests had wrong expectations and failed in CI. This skill codifies the guard that
prevents that from recurring.

### When to Apply

- After writing a new batch of tests
- After any code change that could affect test outcomes
- When setting up a fresh clone of this repo
- When an agent is about to push to main (or any branch feeding main)

### Rule

> **Always run `dotnet test tests/Unit.Tests` locally before pushing. If it fails, fix before pushing.**

### Hook Installation

Install the pre-push hook once per machine/clone. The hook content is below — write it to
`.git/hooks/pre-push` and mark it executable.

**Shell (Linux/macOS/Git Bash):**

```bash
cat > .git/hooks/pre-push << 'EOF'
#!/usr/bin/env bash
# pre-push hook — enforces Unit.Tests pass before any push.
set -euo pipefail

echo "🔎 pre-push: running Unit.Tests…"

if dotnet test tests/Unit.Tests --configuration Release --verbosity quiet 2>&1; then
  echo "✅ pre-push: tests passed — push allowed."
else
  echo ""
  echo "❌ pre-push: Unit tests FAILED. Push aborted."
  echo "   Fix the failures locally before pushing."
  exit 1
fi
EOF
chmod +x .git/hooks/pre-push
```

**PowerShell (Windows):**

```powershell
$hook = @'
#!/usr/bin/env bash
# pre-push hook — enforces Unit.Tests pass before any push.
set -euo pipefail

echo "🔎 pre-push: running Unit.Tests…"

if dotnet test tests/Unit.Tests --configuration Release --verbosity quiet 2>&1; then
  echo "✅ pre-push: tests passed — push allowed."
else
  echo ""
  echo "❌ pre-push: Unit tests FAILED. Push aborted."
  echo "   Fix the failures locally before pushing."
  exit 1
fi
'@
$hook | Set-Content -NoNewline .git/hooks/pre-push
```

> Git hooks live in `.git/hooks/` which is not committed. Every agent or developer who
> clones this repo must run the installation step above once.

### Agent Checklist

Before any `git push`, an agent MUST:

1. Run `dotnet test tests/Unit.Tests --configuration Release --verbosity quiet`
2. Confirm the output contains `Passed!` and `Failed: 0`
3. Only then execute the push

If the hook is already installed (`.git/hooks/pre-push` exists and is executable), step 1
is enforced automatically by git.

### Failure Taxonomy (known patterns)

| Symptom | Root Cause | Fix |
|---------|-----------|-----|
| `DateTime` equality failure in `*.Empty` tests | `Empty` static property calls `DateTime.UtcNow` each time — two calls diverge | Assert individual fields, not whole-record equality |
| Unexpected trailing `_` in slug tests | `GenerateSlug` adds trailing `_` when string ends with punctuation AND has internal punctuation | Check actual slug output against implementation rules before asserting |
| Record equality fails on nested DTO | Nested DTO `Empty` also uses `UtcNow` — same issue | Flatten assertions to field-level |
