# Skill: Build Repair

**Confidence:** high
**Domain:** .NET / CI

## Purpose

Run before EVERY push to catch build/test failures locally before they hit CI.

## Step 0: Locate Solution File

- Check for `*.sln` or `*.slnx` file in current directory
- If not found, run `cd ..` and check again
- Repeat until a `*.sln` or `*.slnx` file is found

## Required Steps (always in this order)

1. `dotnet restore`
2. `dotnet build --configuration Release --no-restore`
3. `dotnet test --configuration Release --no-build`

All three must pass with zero errors AND zero warnings before pushing.

## Error & Warning Resolution

For each error or warning in the build output:
- Identify the affected file and line number
- Research the error/warning code and message
- Apply the recommended fix to the codebase
- Rebuild the solution to verify the fix
- **Repeat until the build completes with zero errors AND warnings**

This is an **iterative process**. Do not assume one pass will fix everything. Keep rebuilding and fixing until clean.

## Test Failure Resolution

If tests fail:
- Identify the failing test(s) and root cause
- Fix the issue in the codebase
- Rebuild and retest
- **Repeat until ALL tests pass**

This is also **iterative**. Do not stop after one fix attempt.

## Verification

Final build output must show:
- "Build succeeded"
- **Zero warnings** (not just zero errors)
- All tests passing

## Documentation

- Create or update `build-log.txt` in the solution directory
- Log all build outputs, error resolutions, and changes made
- Keep a record of the iterative fix process

## Branching Policy (enforced)

- NEVER push directly to `main`
- ALWAYS create a feature branch: `feature/{slug}` or `squad/{issue-number}-{slug}`
- ALWAYS run build-repair steps before pushing the branch
- ALWAYS open a PR — no direct merges

## When to Apply

- Before every `git push`
- After every multi-file change
- After any workflow/config change (especially `.github/`)
