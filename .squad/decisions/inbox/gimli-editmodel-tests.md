# Decision: EditModel POCO Test Pattern

**Date:** 2026  
**Author:** Gimli (Tester)  
**Status:** Accepted

## Context

`StatusEditModel` and `CategoryEditModel` are simple POCO classes defined in the Web project
(`Web.Components.Features.Statuses` and `Web.Components.Features.Categories` namespaces).
They have no validation attributes, no computed properties, and no logic beyond property get/set.

## Decision

For pure POCO edit-model classes, write **5 focused unit tests per model**:

1. `Constructor_SetsExpectedDefaultValues` — verifies all three properties at once
2. `Id_CanBeSet`
3. `{Name}_CanBeSet`
4. `{Description}_CanBeSet`
5. `{Description}_CanBeSetToNull` — explicitly covers the nullable `string?` contract

Tests live in the same namespace as the production class (e.g., `namespace Web.Components.Features.Statuses;`)
because `GlobalUsings.cs` already imports those namespaces project-wide.

No mocking or async is needed for POCO tests; collapsed Arrange/Act with AAA comments is acceptable
when the setup is a single `new Model()` line.

## Files Created

- `tests/Web.Tests.Unit/Components/Features/Statuses/StatusEditModelTests.cs` (5 tests)
- `tests/Web.Tests.Unit/Components/Features/Categories/CategoryEditModelTests.cs` (5 tests)

## Outcome

All 10 tests pass (`dotnet test --filter "FullyQualifiedName~EditModelTests"` → 10/10).
