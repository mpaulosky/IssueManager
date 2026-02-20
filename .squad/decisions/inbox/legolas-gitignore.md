# Decision: Create .gitignore for IssueManager

**Date:** 2026-02-17  
**By:** Legolas (DevOps)  
**Status:** IMPLEMENTED  

## What
Created `.gitignore` at repository root to prevent accidental commits of:
- Build artifacts (`bin/`, `obj/`, `*.dll`, `*.exe`)
- Secrets (`.env`, `appsettings.*.local.json`, `user-secrets/`)
- Local MongoDB data (`.mongo/`, `mongodb-data/`)
- IDE user files (`.csproj.user`, `.idea/`)
- Logs and test artifacts (`logs/`, `coverage/`, `.trx`)
- Aspire debug manifests

## Why
**Security Risk:**
- `.env` files contain database passwords, API keys, and authentication tokens — must never be committed
- `appsettings.Development.local.json` may include local override secrets

**Quality Risk:**
- Build outputs and test artifacts clutter the repository and waste storage
- IDE user-specific settings create noise in diffs and merge conflicts
- Local MongoDB data should never be version controlled

**Developer Experience:**
- Clear patterns allow developers to safely use local `.env` and config files without risk of accidental commits
- Reduces diff noise in pull reviews

## Scope
- Does NOT exclude `.ai-team/` or `.github/` — these are intentionally version controlled for squad automation
- Covers .NET, C#, Aspire, Blazor, MongoDB, and Node tooling patterns
- Includes macOS (`.DS_Store`) and Windows (`Thumbs.db`) OS-specific files

## Impact
- All developers should do `git pull` to get `.gitignore` and refresh their local working trees
- Secrets/artifacts already committed must be remediated separately (rotate keys, remove from history if sensitive)
