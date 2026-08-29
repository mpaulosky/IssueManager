# Scripts

Utility scripts for repository maintenance and developer workflows.

## New-BlogPost.ps1

Generates a Markdown blog post summarizing a commit's message and diff, writes it to
`docs/blogs/`, and stages it with `git add` so it ships in the same push as the change it
documents.

### Usage

```powershell
# Summarize the current HEAD commit
.\scripts\New-BlogPost.ps1

# Summarize a specific commit with a custom title
.\scripts\New-BlogPost.ps1 -Ref HEAD~1 -Title "Consolidated Mongo repositories"
```

Run it after committing your change and before pushing — the generated post is staged, so
include it in the same commit (`git commit --amend`) or a follow-up commit before you push.

## New-ReleaseBlogPost.ps1

Generates a Markdown blog post documenting a GitHub release (via `gh release view`) and stages
it in `docs/blogs/`. Requires the GitHub CLI (`gh`) authenticated against this repo.

### Usage

```powershell
# Document the latest release
.\scripts\New-ReleaseBlogPost.ps1

# Document a specific release
.\scripts\New-ReleaseBlogPost.ps1 -Tag v0.0.19

# Backfill posts for every release (skips any that already exist)
.\scripts\New-ReleaseBlogPost.ps1 -All
```

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
