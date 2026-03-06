# Decision: Test Workflow Restructuring — Individual Jobs Per Test Project

**Date:** 2026-03-09  
**Author:** Boromir (DevOps)  
**Status:** Implemented  
**Commit:** cffe0b1  

## What

Rewrote `.github/workflows/squad-test.yml` to run each test project in its own dedicated job instead of bundling multiple projects in bash scripts.

## Changes

### 1. Split test-unit Job (3 new jobs)
**Before:**
- Single `test-unit` job ran Api.Tests.Unit + Shared.Tests.Unit in bash script
- Combined artifact: `unit-test-results`

**After:**
- `test-api-unit` — runs Api.Tests.Unit
- `test-shared-unit` — runs Shared.Tests.Unit
- `test-web-unit` — runs Web.Tests.Unit (0 tests currently, placeholder)
- Separate artifacts: `api-unit-test-results`, `shared-unit-test-results`, `web-unit-test-results`

### 2. Replace test-aspire Job
**Before:**
- `test-aspire` job referenced deleted `tests/Aspire/` project
- Used xUnit v3 executable workaround

**After:**
- `test-apphost-unit` job for AppHost.Tests.Unit
- Uses standard `dotnet test` (NOT xUnit executable — AppHost tests don't have TestProcessLauncherAdapter issue)
- Gracefully skips if directory missing with `::notice::` + `exit 0`

### 3. Job Naming Standardization
**Before:**
- "Unit Tests", "Blazor Component Tests", "Integration Tests", "Architecture Tests", "Aspire Tests"

**After:**
- "Api.Tests.Unit", "Shared.Tests.Unit", "Web.Tests.Unit", "Architecture.Tests", "Web.Tests.Bunit", "Api.Tests.Integration", "AppHost.Tests.Unit"
- All names match actual test project names

### 4. Updated Dependencies
Both `coverage` and `report` jobs now depend on all 7 test jobs:
```yaml
needs:
  - test-api-unit
  - test-shared-unit
  - test-web-unit
  - test-architecture
  - test-bunit
  - test-integration
  - test-apphost-unit
```

### 5. squad-ci.yml Integration
**Before:**
- TODO placeholder with no real commands

**After:**
- Calls `squad-test.yml` via `workflow_call` for DRY pipeline
- Single source of truth for build/test logic

## Why

1. **Parallelization:** Individual jobs run in parallel, reducing total CI time
2. **Clearer failure reporting:** Failed test project immediately visible in job list
3. **Artifact isolation:** Each project's results uploaded separately for easier debugging
4. **Alignment with project split:** Reflects Gimli's Unit.Tests → Api.Tests.Unit/Shared.Tests.Unit/Web.Tests.Unit refactoring (PR #95)
5. **Stale reference cleanup:** Removes deleted tests/Aspire/ project reference
6. **Naming consistency:** Job names match test project names for predictability

## Test Project Status

| Project               | Status                    | Tests | Job Name            |
|-----------------------|---------------------------|-------|---------------------|
| Api.Tests.Unit        | ✅ Active                 | ~200  | test-api-unit       |
| Shared.Tests.Unit     | ✅ Active                 | ~150  | test-shared-unit    |
| Web.Tests.Unit        | ⚠️ Placeholder (0 tests) | 0     | test-web-unit       |
| Architecture.Tests    | ✅ Active                 | 9     | test-architecture   |
| Web.Tests.Bunit       | ✅ Active                 | ~164  | test-bunit          |
| Api.Tests.Integration | ✅ Active                 | ~50   | test-integration    |
| AppHost.Tests.Unit    | ✅ Active                 | ~18   | test-apphost-unit   |

## Key Implementation Details

### Web.Tests.Unit (0 tests)
- Job runs successfully with 0 tests (expected behavior)
- No special handling needed — `dotnet test` succeeds on empty test projects
- Placeholder for future Web component unit tests

### AppHost.Tests.Unit (dotnet test)
- Uses standard `dotnet test`, NOT xUnit executable workaround
- AppHost tests don't have TestProcessLauncherAdapter compatibility issue
- Graceful skip pattern if directory missing (Aspire/Docker infrastructure may not be available in all CI environments)

### Test Execution Pattern
All unit test jobs follow same structure:
1. Checkout + Setup .NET + Cache packages
2. Restore dependencies
3. Check directory exists (error if missing for Api/Shared, notice if missing for AppHost)
4. Run `dotnet test` with code coverage, TRX logger, minimal verbosity
5. Upload artifact with job-specific name

## Related History

- **2026-03-07:** Gimli split Unit.Tests → Api.Tests.Unit/Shared.Tests.Unit/Web.Tests.Unit (PR #95)
- **2026-02-28:** xUnit v3 migration (3.2.2) — some projects require executable workaround, others work with `dotnet test`
- **2026-03-06:** Issue #89 Aspire startup fixes validated (tests/Aspire/ was old project name)

## Future Considerations

- When Web.Tests.Unit gets tests, no workflow changes needed — job already configured
- If AppHost.Tests.Unit develops TestProcessLauncherAdapter issues, switch to xUnit executable pattern (unlikely — Aspire tests have been stable)
- Consider adding test count validation step to detect accidentally deleted test files

## Files Modified

- `.github/workflows/squad-test.yml` — complete job restructuring
- `.github/workflows/squad-ci.yml` — removed TODO, added workflow_call to squad-test.yml
