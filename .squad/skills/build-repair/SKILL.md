# Skill: Build Repair

**Confidence:** high
**Domain:** .NET / CI

## Purpose

Run before EVERY push to catch build/test failures locally before they hit CI.

## Required Steps (always in this order)

1. `dotnet restore`
2. `dotnet build --configuration Release`
3. `dotnet test --configuration Release --no-build`

All three must pass with zero errors before pushing.

## Branching Policy (enforced)

- NEVER push directly to `main`
- ALWAYS create a feature branch: `feature/{slug}` or `squad/{issue-number}-{slug}`
- ALWAYS run build-repair steps before pushing the branch
- ALWAYS open a PR — no direct merges

## When to Apply

- Before every `git push`
- After every multi-file change
- After any workflow/config change (especially `.github/`)
