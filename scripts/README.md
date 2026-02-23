# Scripts

Utility scripts for repository maintenance and developer workflows.

## cleanup-merged-branches.ps1

Automatically cleans up local branches whose remote tracking branches have been deleted (typically after PR merge).

### What it does

1. Fetches from origin with `--prune` to update remote-tracking refs
2. Identifies local branches tracking a deleted remote (`: gone]` status)
3. Safely deletes those branches using `git branch -d`
4. Skips protected branches (`main`, `develop`)
5. Reports deleted and skipped branches

### Usage

```powershell
# Dry run - see what would be deleted
.\scripts\cleanup-merged-branches.ps1 -DryRun

# Delete merged branches safely
.\scripts\cleanup-merged-branches.ps1

# Force delete (even if not fully merged)
.\scripts\cleanup-merged-branches.ps1 -Force
```

### Git Alias

A convenient `git gone` alias is configured in the repository:

```bash
git gone          # Run cleanup
git gone -DryRun  # Preview changes
git gone -Force   # Force cleanup
```

### Parameters

- **`-DryRun`**: Preview which branches would be deleted without making changes
- **`-Force`**: Force-delete branches even if they appear unmerged locally

### Example Output

```
🔄 Fetching from origin with prune...
🔍 Scanning for orphaned local branches...

📋 Orphaned branches to remove:
  - feature/old-work
  - bugfix/issue-123

  ✅ Deleted: feature/old-work
  ✅ Deleted: bugfix/issue-123

📊 Summary:
  Deleted: 2 | Skipped: 0
```
