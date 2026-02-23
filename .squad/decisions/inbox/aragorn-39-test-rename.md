# Decision: Rename Test Folders to .Tests Convention

**Issue:** #39
**Branch:** squad/39-rename-test-folders
**PR:** https://github.com/mpaulosky/IssueManager/pull/44
**Date:** 2025
**Author:** Aragorn (Lead Developer)

## Decision

Rename all test folders and projects under `tests/` to consistently end in `.Tests`, matching standard .NET test project naming conventions.

## Changes Made

| Old | New |
|-----|-----|
| `tests/Architecture/Architecture.csproj` | `tests/Architecture.Tests/Architecture.Tests.csproj` |
| `tests/BlazorTests/BlazorTests.csproj` | `tests/Blazor.Tests/Blazor.Tests.csproj` |
| `tests/Integration/Integration.csproj` | `tests/Integration.Tests/Integration.Tests.csproj` |
| `tests/Unit/Unit.csproj` | `tests/Unit.Tests/Unit.Tests.csproj` |

`IssueManager.sln` updated with corrected project names and paths for all 4 projects.

## Rationale

- Consistency: all test projects now follow the `<ProjectName>.Tests` convention.
- Discoverability: IDEs and CI tooling can identify test projects by name suffix.
- `Aspire` test project was left unchanged as it was not in scope for this issue.

## Notes

- `RootNamespace` values in `.csproj` files were intentionally left unchanged to avoid breaking existing `using` statements.
- On Windows, `Rename-Item` may fail with "Access Denied" for directories; `Move-Item` is a reliable alternative.
- Integration tests require Docker (TestContainers + MongoDB 8.0); excluded from local verification run.

## Outcome

Build: 0 errors, 0 warnings. 84 tests passing (Unit: 62, Architecture: 9, Blazor: 13).
