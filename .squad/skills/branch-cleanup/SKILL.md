# Branch Cleanup — Routine Local Branch Maintenance

**Confidence:** `medium`  
**Last Updated:** 2026-02-24  
**Owner:** Boromir (DevOps / Infrastructure)

---

## Purpose

Maintain a clean local workspace by removing orphaned local branches after PRs merge to main. Prevents accumulation of stale `squad/*` branches while **absolutely protecting** the `main` and `develop` branches from deletion.

## Pattern

### Safe Cleanup Script (with Origin Tracking)

```bash
#!/bin/bash
# safe-branch-cleanup.sh — Remove merged branches + branches deleted from origin

set -e

echo "🧹 Branch Cleanup Starting..."
echo ""

# Step 1: Fetch latest from remote
echo "📡 Fetching latest from remote..."
git fetch --all --prune 2>/dev/null

# Step 2: List branches that are merged into main
echo ""
echo "📋 Branches merged into main (candidates for deletion):"
MERGED_BRANCHES=$(git branch --merged main --list "squad/*" | sed 's/^[ *]*//')
echo "$MERGED_BRANCHES"

# Step 3: List branches deleted from origin but still local
echo ""
echo "🗑️  Branches deleted from origin (still local):"
GONE_BRANCHES=$(git branch -v | grep '\[gone\]' | awk '{print $1}')
echo "$GONE_BRANCHES"

# Step 4: Combine both lists for deletion
echo ""
echo "🔍 Total candidates for deletion:"
ALL_CLEANUP=$(echo -e "$MERGED_BRANCHES\n$GONE_BRANCHES" | grep -v '^$' | sort -u)

if [ -z "$ALL_CLEANUP" ]; then
  echo "✅ No branches to clean up."
  exit 0
fi

echo "$ALL_CLEANUP"

# Step 5: Delete with confirmation
echo ""
read -p "Delete these branches? (y/N) " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
  echo "$ALL_CLEANUP" | while read branch; do
    if [[ "$branch" != "main" && "$branch" != "develop" && -n "$branch" ]]; then
      echo "  ❌ Deleting: $branch"
      git branch -d "$branch" 2>/dev/null || git branch -D "$branch" 2>/dev/null
    fi
  done
  echo "✅ Cleanup complete."
else
  echo "⏭️  Cleanup skipped."
fi
```

### Manual Cleanup (Safe Steps)

**Step 1:** Fetch and prune
```bash
git fetch --all --prune
```

**Step 2:** List branches merged into main
```bash
git branch --merged main
```

**Step 3:** Check for deleted origin branches
```bash
# List local branches that no longer exist on origin
git branch -v | grep "\[gone\]"

# Or, compare local vs remote explicitly
comm -23 <(git branch | sed 's/[ *]*//') <(git branch -r | grep -v HEAD | sed 's|origin/||' | sort)
```

**Step 4:** Delete individual branches (with safeguards)
```bash
# Safe: deletes only if fully merged
git branch -d squad/51-test-fixes-phase-1

# Force delete if needed (use with caution)
git branch -D squad/51-test-fixes-phase-1
```

**Step 5:** Verify main is untouched
```bash
git branch -v | grep main
# Output: * main abc1234 docs(...) ← Should show current commit
```

## Safety Rules (Non-Negotiable)

1. **`main` branch is always protected.**
   - NEVER add `main` to any cleanup script
   - NEVER pass `--all` without explicit exclusion of `main`
   - Verify with: `git branch --merged main | grep -c "^[ *]*main$"` (should output 0)

2. **`develop` branch (if it exists) is also protected.**
   - Add exclusion: `--list "squad/*"` (pattern-match only squad branches)
   - If custom branches exist, add them to the exclusion list

3. **Two types of cleanup to identify:**
   - **Merged into main:** Branches with complete work, safe to delete
   - **Deleted from origin:** Remote was pruned but local copy remains (tracking `[gone]`)

4. **Ask before deleting.**
   - Always list candidates first (both merged AND gone)
   - Require confirmation (`read -p "Delete...?"`) before any deletion
   - Show the branch name in the delete message

5. **Use `-d` first (safe delete), then `-D` if needed.**
   - `-d` = refuse to delete if not fully merged (safe)
   - `-D` = force delete regardless (use only if you're sure)

## Integration Points

**When to run cleanup:**
- After PR merges (manual or as part of post-merge workflow)
- Weekly routine (schedule via GitHub Actions cron if desired)
- Before starting new feature branches (to keep the list short)

**Who should run it:**
- Boromir (DevOps) — owns the cleanup script and automation
- Any squad member locally, with confirmation prompt

**Automation example (GitHub Actions):**
```yaml
name: Cleanup Merged Branches
on:
  schedule:
    - cron: "0 9 * * 1"  # Weekly Monday 9 AM UTC
  workflow_dispatch:

jobs:
  cleanup:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0
      - name: Delete merged branches
        run: |
          git fetch --all --prune
          git branch --merged main --list "squad/*" | grep -v "^ *main$" | xargs -r git branch -d
```

## Examples

### Example 1: Identify branches deleted from origin
```bash
$ git fetch --all --prune
$ git branch -v | grep '\[gone\]'
  squad/old-feature      a1b2c3d [gone] Old feature branch
  squad/stale-branch     d4e5f6g [gone] Stale branch

# These branches no longer exist on origin — safe to delete locally
```

### Example 2: Delete single merged branch
```bash
$ git branch --merged main | grep squad
  squad/51-test-fixes-phase-1

$ git branch -d squad/51-test-fixes-phase-1
Deleted branch squad/51-test-fixes-phase-1 (was 28948f0).

$ git branch -v | grep squad
# (no output — branch deleted)
```

### Example 3: Batch cleanup (merged + gone branches)
```bash
$ git fetch --all --prune

$ git branch --merged main --list "squad/*"
  squad/33-status-category-comment-handlers
  squad/51-test-fixes-phase-1

$ git branch -v | grep '\[gone\]'
  squad/old-feature      a1b2c3d [gone]
  squad/stale-branch     d4e5f6g [gone]

$ git branch -d squad/33-status-category-comment-handlers
$ git branch -d squad/51-test-fixes-phase-1
$ git branch -D squad/old-feature
$ git branch -D squad/stale-branch

✅ All 4 branches deleted.
```

### Example 4: Protected main branch (verification)
```bash
$ git branch --merged main | head -5
* main
  squad/51-test-fixes-phase-1

$ git branch --merged main --list "squad/*"
  squad/51-test-fixes-phase-1
# (main is NOT in the list — protection works)
```

## References

- `git branch -d` docs: https://git-scm.com/docs/git-branch#Documentation/git-branch.txt--d
- GitHub Actions workflow syntax: https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions
- Fetch-prune best practice: https://git-scm.com/docs/git-fetch#Documentation/git-fetch.txt---prune

## Confidence Justification

**`medium`** confidence because:
- ✅ Pattern is battle-tested (standard git cleanup workflow)
- ✅ Safety rules are explicit (main protection is clear)
- ✅ Multiple agents independently confirmed this pattern works
- ⏳ Waiting for Boromir to operationalize (GitHub Actions scheduling + team validation)

Will bump to `high` after Boromir runs the workflow successfully in 2-3 production cycles.
