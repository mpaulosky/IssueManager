# Boromir — History

## Core Context
DevOps on IssueManager (.NET 10, GitHub Actions, Aspire, NuGet centralized packages). User: Matthew Paulosky.

## Learnings

### 2026-02-25: NuGet.config cross-platform fix
- Removed Windows-only `<config>` block from `NuGet.config` that set `globalPackagesFolder` to `%USERPROFILE%\.nuget\packages_aspire`
- This caused `MSB4019` error on Linux CI runners (path not expanded)
- Fixed in commit `26b3e73` on PR #54

### 2026-02-25: Protected Branch Guard
- Workflow: `.github/workflows/` (guard checks `.squad/` files in PR diff on non-exempt branches)
- Only `squad/*` branches are exempt from the guard
- `feature/*` branches: if `.squad/` files appear in the diff, the CI check fails

### Key files
- `Directory.Packages.props` — all NuGet package versions
- `NuGet.config` — must be cross-platform (no `%USERPROFILE%` paths)
- `.github/workflows/` — GitHub Actions CI definitions
- `src/AppHost/` — Aspire AppHost project
- `src/ServiceDefaults/` — Aspire ServiceDefaults project
