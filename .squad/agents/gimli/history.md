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

### 2026-02-27: Sprint 2 — bUnit CRUD page tests written
- Wrote 11 test files covering 10 CRUD pages + FooterComponent
- Target: `tests/Blazor.Tests/Pages/Issues/`, `Pages/Categories/`, `Pages/Statuses/`, `Layout/`
- Files: `IssuesPageTests`, `CreateIssuePageTests`, `IssueDetailPageTests`, `EditIssuePageTests`, `CategoriesPageTests`, `CreateCategoryPageTests`, `EditCategoryPageTests`, `StatusesPageTests`, `CreateStatusPageTests`, `EditStatusPageTests`, `FooterComponentTests`
- **`BuildInfo` is `internal` to Web assembly** — test project cannot access it. Use markup assertions (e.g. `Contain("github.com")`) instead of `BuildInfo.Version` / `BuildInfo.Commit`.
- **Re-registering services**: When a test class inherits `ComponentTestBase` (which pre-registers `ICategoryApiClient`/`IStatusApiClient`), calling `AddSingleton<T>()` again in the subclass constructor AFTER base constructor completes causes the new registration to win in DI. This is the correct pattern for overriding mocks in subclasses.
- **Category/Status page tests don't need ComponentTestBase** — those pages don't render `IssueForm` (which injects category/status services). Use fresh `new TestContext()` for clean isolation.
- **Issue pages that render `IssueForm`** (CreateIssuePage, EditIssuePage) DO need `ICategoryApiClient` + `IStatusApiClient` — inherit from `ComponentTestBase`.
- **IssuesPage** also needs `ICategoryApiClient`/`IStatusApiClient` (for filter dropdowns) — inherit from `ComponentTestBase`.
- All test classes follow IDisposable pattern (or inherit ComponentTestBase which is IDisposable).
- Build result: `0 Warning(s). 0 Error(s).`

### 2026-02-27: Search/filter feature tests written
- Wrote comprehensive tests for Sam's search/filter feature (SearchTerm, AuthorName parameters)
- Unit tests: Added 6 new validator tests to `ListIssuesQueryValidatorTests.cs` for SearchTerm/AuthorName validation (200 char limit)
- API client tests: Added 4 new tests to `IssueApiClientTests.cs` to verify URL query string construction with filters
- Integration tests: Created new `IssueRepositorySearchTests.cs` with 10 comprehensive tests for MongoDB filtering behavior
- Tests written against expected API after Sam's changes (ListIssuesQuery with SearchTerm?/AuthorName? properties)
- MockHandler enhanced to capture LastRequest for URL assertion in API client tests
- All tests follow AAA pattern, use FluentAssertions `.Should()`, and include proper file headers
- Integration tests validate: case-insensitive search, description matching, author filtering, combined filters with pagination, empty results
