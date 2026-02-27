# Gimli — History

## Core Context
Tester on IssueManager (.NET 10, xUnit, FluentAssertions, NSubstitute, bUnit, TestContainers). User: Matthew Paulosky.

## Learnings

### 2026-02-25: Shared project test batch
- Wrote 11 test files for DTOs (7), Exceptions (2), Helpers (2) — commit `04714a4`
- Test project: `tests/Unit.Tests/`
- RootNamespace = `Unit`, so namespaces are `Tests.Unit.DTOs`, `Tests.Unit.Helpers`, etc.
- Global usings: `Xunit`, `FluentAssertions`, `NSubstitute`, `FluentValidation`

### 2026-02-25: Two tests were wrong
- `CommentDtoTests.Empty_ReturnsInstanceWithDefaultValues` — asserted `dto.Issue.Should().Be(IssueDto.Empty)` → FAILED because `IssueDto.Empty` is not a singleton (calls `DateTime.UtcNow` each access)
- `HelpersTests.GenerateSlug_CSharpIsGreat` — expected `"c_is_great"` → FAILED because `GenerateSlug` correctly produces `"c_is_great_"` (trailing underscore for punctuation-terminated input)
- Fixed in commit `5aea3e2` — lesson: always run tests locally before pushing

### Test project structure
- `tests/Unit.Tests/` — unit tests (fast, no external deps)
  - `DTOs/`, `Exceptions/`, `Helpers/`, `Repositories/`, `Endpoints/`
- `tests/Integration.Tests/` — Docker-backed MongoDB tests (ALL classes need `[Collection("Integration")]`)
- `tests/Architecture.Tests/` — architecture constraint tests
- `tests/Web.Tests.Bunit/` — Blazor component tests

### Key patterns
- `IssueDto.Empty` and `CommentDto.Empty` use `DateTime.UtcNow` — never compare two calls directly
- `GenerateSlug("C# Is Great!")` → `"c_is_great_"` (trailing underscore is correct and intentional)
- Integration tests: always add `[Collection("Integration")]` to prevent Docker port conflicts

### 2026-02-27: Search/filter feature tests written
- Wrote comprehensive tests for Sam's search/filter feature (SearchTerm, AuthorName parameters)
- Unit tests: Added 6 new validator tests to `ListIssuesQueryValidatorTests.cs` for SearchTerm/AuthorName validation (200 char limit)
- API client tests: Added 4 new tests to `IssueApiClientTests.cs` to verify URL query string construction with filters
- Integration tests: Created new `IssueRepositorySearchTests.cs` with 10 comprehensive tests for MongoDB filtering behavior
- Tests written against expected API after Sam's changes (ListIssuesQuery with SearchTerm?/AuthorName? properties)
- MockHandler enhanced to capture LastRequest for URL assertion in API client tests
- All tests follow AAA pattern, use FluentAssertions `.Should()`, and include proper file headers
- Integration tests validate: case-insensitive search, description matching, author filtering, combined filters with pagination, empty results
