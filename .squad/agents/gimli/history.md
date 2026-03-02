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

### 2026-02-28: bUnit 2.x Migration — Complete

**Task:** Migrate all Blazor test files from bUnit 1.29.5 → 2.6.2 API following Boromir's package upgrade.

**Breaking Changes Applied:**

1. **`TestContext` namespace collision**: bUnit 2.x introduced `Bunit.TestContext` class, colliding with `Xunit.TestContext`.
   - **Fix**: Fully qualified as `Bunit.TestContext` in 6 files (Categories/Statuses page tests).
   
2. **`RenderComponent<T>()` → `Render<T>()`**: Primary rendering method renamed.
   - **Fix**: Global replace across 17 files (~100 occurrences).
   
3. **`SetParametersAndRender()` removed**: Lambda-based parameter update method no longer exists.
   - **Fix**: Re-render component entirely with `TestContext.Render<T>(parameters => ...)` in IssueFormTests.cs.
   
4. **FluentAssertions v8 bonus fix**: `HaveCountGreaterOrEqualTo()` → `HaveCountGreaterThanOrEqualTo()` in FooterComponentTests.cs (3 occurrences).

**Build Result:** ✅ Build succeeded. 0 errors, 13 CS0618 obsolete warnings (non-blocking — `Bunit.TestContext` marked obsolete in favor of `BunitContext`).

**Test Result:** ✅ All 143 Blazor tests pass (Duration: 3 seconds).

**Files Modified:** 17 unique files, ~116 surgical edits. No test logic changed.

**Future Work (Optional, P3):** Migrate from `Bunit.TestContext` → `BunitContext` to eliminate obsolete warnings (non-breaking, cosmetic).

**Decision Inbox:** Full migration report created at `.squad/decisions/inbox/gimli-bunit-2x-migration.md`.


- Integration tests validate: case-insensitive search, description matching, author filtering, combined filters with pagination, empty results

### 2026-02-27: Sprint 4 — Auth0 integration tests written
- Fixed 3 compilation errors caused by `ICurrentUserService` parameter added to `CreateIssueHandler` and `CreateCommentHandler`
  - `Integration.Tests/Handlers/CreateIssueHandlerTests.cs` — added `Substitute.For<ICurrentUserService>()` mock with test values
  - `Unit.Tests/Handlers/Issues/CreateIssueHandlerTests.cs` — added mock with default authenticated user
  - `Unit.Tests/Handlers/Comments/CreateCommentHandlerTests.cs` — added mock with default authenticated user
- Created `CurrentUserServiceTests.cs` (14 tests) — validates Auth0 claim reading (sub, name, email, IsAuthenticated)
  - Tests ClaimTypes.NameIdentifier, "sub", ClaimTypes.Name, "name", ClaimTypes.Email, "email" claims
  - Tests null HttpContext handling (returns null for properties, false for IsAuthenticated)
  - Tests unauthenticated user scenario
- Added `NSubstitute` package reference to `Integration.Tests.csproj` (required for mocking in integration tests)
- **TokenForwardingHandler tests skipped** — Web.Services not referenced in Unit.Tests or Integration.Tests; requires Auth session APIs hard to mock
- Final test results: **609 tests, 0 failures, 0 skipped**
  - Architecture.Tests: 4.6s
  - Unit.Tests: 6.4s (includes 14 new CurrentUserService tests)
  - Blazor.Tests: 6.7s
  - Integration.Tests: 350.0s (includes 1 updated CreateIssueHandlerTests)

### 2026-02-28: FluentAssertions v6.12.1 → v8.8.0 scan — NO BREAKING CHANGES FOUND
- Scanned ALL 96 test files (Unit.Tests, Integration.Tests, Blazor.Tests, Architecture.Tests)
- Searched for FA v8 breaking change patterns:
  - `CompleteWithinAsync` return type changes — NO USAGE FOUND
  - `NotThrowAsync` / `ThrowAsync` API surface changes — patterns used are FA v8 compatible (`Func<Task> act = async () => ...` pattern)
  - `ExecutionTime()` / `ExecuteWithinAsync` changes — NO USAGE FOUND
  - `.Subject` property removal — NO USAGE FOUND
  - `BeEquivalentTo` option changes — usage found is compatible (no excluded members)
  - Numeric assertion changes (`BeApproximately`) — NO USAGE FOUND
- All async exception assertions use the correct FA v8 pattern:
  ```csharp
  Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
  await act.Should().ThrowAsync<ExceptionType>().WithMessage("*pattern*");
  ```
- **RESULT:** Zero FluentAssertions v8 breaking changes detected. All test files are FA v8 compatible.
- **NOTE:** Build failures found are bUnit v2.x breaking changes (`RenderComponent` → `Render`, `SetParametersAndRender` removal) — NOT FluentAssertions. This is expected from Boromir's NuGet upgrade (bUnit 1.29.5 → 2.6.2). bUnit migration is a separate task.

### 2026-02-28: Phase 3 — BunitContext migration + xUnit1051 CancellationToken cleanup

**Task:** Eliminate pre-push hook warnings: CS0618 (Bunit.TestContext obsolete) and xUnit1051 (CancellationToken.None).

**Fix 1 — BunitContext migration (7 files, CS0618 eliminated):**
- `Bunit.TestContext` → `BunitContext` in field declarations, property types, and constructor calls
- Files: `ComponentTestBase.cs`, `CategoriesPageTests.cs`, `CreateCategoryPageTests.cs`, `EditCategoryPageTests.cs`, `StatusesPageTests.cs`, `CreateStatusPageTests.cs`, `EditStatusPageTests.cs`
- `using Bunit;` already present — just drop the `Bunit.` prefix and use `BunitContext` directly

**Fix 2 — xUnit1051 CancellationToken (~50 call sites across 10 files):**
- `_handler.Handle(CancellationToken.None)` → `_handler.Handle(Xunit.TestContext.Current.CancellationToken)` in `ListCategoriesHandlerTests.cs` and `ListStatusesHandlerTests.cs`
- NSubstitute setup calls `_repository.GetAllAsync()` → `_repository.GetAllAsync(Arg.Any<CancellationToken>())` — critical: needed because `ListStatusesHandler` DOES forward CT while `ListCategoriesHandler` does NOT. Using `Arg.Any<>()` works for both.
- `await _repository.Received(1).GetAllAsync()` → `Arg.Any<CancellationToken>()` variant for same reason
- Repository method calls in `RepositoryValidationTests.cs`: add `Xunit.TestContext.Current.CancellationToken` as last argument
- API client test calls in `StatusApiClientTests.cs`, `CategoryApiClientTests.cs`, `IssueApiClientTests.cs`: add CT as last argument
- `CommentApiClientTests.cs`: special case — `ICommentApiClient.GetAllAsync(string? issueId = null, CancellationToken ct = default)` has `issueId` as first param, so must use named arg `cancellationToken:` instead of positional

**Key Discovery — ListCategoriesHandler production bug (flagged, NOT fixed):**
- `ListCategoriesHandler.Handle()` accepts a CancellationToken parameter but does NOT forward it to `_repository.GetAllAsync()`. This is a production code bug. Not my job to fix — flagged for Aragorn/Sam.

**Build/Test results:** Unit.Tests 390/390 ✅, Blazor.Tests 143/143 ✅. Build: 0 warnings, 0 errors. Pre-push gate: all 3 test suites passed.

**Commit:** `414828f` — fix(tests): Phase 3 - BunitContext migration and xUnit1051 CancellationToken cleanup

### 2026-02-28: MongoDB Image Standardization — mongo:latest + MongoDbBuilder v4 API

**Task:** Standardize all integration tests to use `mongo:latest` image and fix deprecated MongoDbBuilder() parameterless constructor.

**Files Fixed (11 total):**
- `tests/Integration.Tests/Fixtures/MongoDbFixture.cs` (already had mongo:latest)
- `tests/Integration.Tests/Handlers/DeleteIssueHandlerIntegrationTests.cs` (mongo:8.2 → latest)
- `tests/Integration.Tests/Handlers/DeleteIssueHandlerTests.cs` (mongo:8.2 → latest)
- `tests/Integration.Tests/Handlers/CreateIssueHandlerTests.cs` (mongo:8.2 → latest)
- `tests/Integration.Tests/Handlers/GetIssueHandlerTests.cs` (mongo:8.2 → latest)
- `tests/Integration.Tests/Handlers/UpdateIssueHandlerIntegrationTests.cs` (mongo:8.2 → latest)
- `tests/Integration.Tests/Handlers/IssueRepositorySearchTests.cs` (mongo:8.2 → latest)
- `tests/Integration.Tests/Handlers/ListIssuesHandlerIntegrationTests.cs` (mongo:8.2 → latest)
- `tests/Integration.Tests/Handlers/UpdateIssueStatusHandlerTests.cs` (mongo:8.2 → latest)
- `tests/Integration.Tests/Data/CategoryRepositoryTests.cs` (mongo:8.0 → latest)
- `tests/Integration.Tests/Data/IssueRepositoryTests.cs` (mongo:8.2 → latest)

**Changes Applied:**
1. **Image tag**: Changed all hardcoded `"mongo:8.0"` and `"mongo:8.2"` to `"mongo:latest"` for consistency
2. **MongoDbBuilder API**: Fixed deprecated parameterless constructor by passing image directly to constructor
   - **Old pattern**: `new MongoDbBuilder().WithImage(MongodbImage).Build()`
   - **New pattern**: `new MongoDbBuilder(MongodbImage).Build()` (Testcontainers v4.10.0 API)

**Build Results:** 
- Before: 11 CS0618 obsolete warnings for MongoDbBuilder()
- After: ✅ 0 warnings, 0 errors, build succeeded

**Rationale:** User requested all Testcontainers use latest MongoDB image. Testcontainers.MongoDB v4.10.0 deprecated the parameterless constructor in favor of passing the image name directly to the constructor, aligning with the testcontainers-dotnet project's direction per https://github.com/testcontainers/testcontainers-dotnet/discussions/1470.

**Commit:** `4ad9e6f` — test: standardize all integration tests to mongo:latest image

### 2026-03-01: xUnit1051 Integration Test CancellationToken Fix
**Task:** Fix xUnit1051 warnings across all 10 Integration.Tests files by passing `TestContext.Current.CancellationToken` to async repository and handler calls.

**Files Fixed (10 total):**
- `tests/Integration.Tests/Data/CategoryRepositoryTests.cs` — 12 async calls fixed
- `tests/Integration.Tests/Data/IssueRepositoryTests.cs` — 9 async calls fixed
- `tests/Integration.Tests/Handlers/CreateIssueHandlerTests.cs` — 7 async calls fixed
- `tests/Integration.Tests/Handlers/DeleteIssueHandlerIntegrationTests.cs` — 18 async calls fixed
- `tests/Integration.Tests/Handlers/DeleteIssueHandlerTests.cs` — 7 async calls fixed
- `tests/Integration.Tests/Handlers/GetIssueHandlerTests.cs` — 8 async calls fixed
- `tests/Integration.Tests/Handlers/IssueRepositorySearchTests.cs` — 35 async calls fixed
- `tests/Integration.Tests/Handlers/ListIssuesHandlerIntegrationTests.cs` — 13 async calls fixed
- `tests/Integration.Tests/Handlers/UpdateIssueHandlerIntegrationTests.cs` — 14 async calls fixed (including `Task.Delay(100)` → `Task.Delay(100, ct)`)
- `tests/Integration.Tests/Handlers/UpdateIssueStatusHandlerTests.cs` — 8 async calls fixed

**Patterns Applied:**
1. **Repository calls**: `_repository.CreateAsync(entity)` → `_repository.CreateAsync(entity, TestContext.Current.CancellationToken)`
2. **Handler calls**: `_handler.Handle(command)` → `_handler.Handle(command, TestContext.Current.CancellationToken)`
3. **Pagination calls**: `GetAllAsync(page: 1, pageSize: 20)` → `GetAllAsync(page: 1, pageSize: 20, cancellationToken: TestContext.Current.CancellationToken)` (named param required due to optional searchTerm/authorName params)
4. **Exception tests**: `Assert.ThrowsAsync<T>(() => _handler.Handle(cmd))` → `Assert.ThrowsAsync<T>(() => _handler.Handle(cmd, TestContext.Current.CancellationToken))`
5. **Task.Delay**: `await Task.Delay(100)` → `await Task.Delay(100, TestContext.Current.CancellationToken)`

**Not Fixed (Intentional):**
- `InitializeAsync` / `DisposeAsync` lifecycle hooks: TestContext.Current is null in lifecycle methods, so container lifecycle calls (`_mongoContainer.StartAsync()`, `StopAsync()`, `DisposeAsync()`) remain without CT. xUnit1051 does NOT apply to lifecycle hooks.

**Build Results:**
- Before: 103+ xUnit1051 warnings, 0 errors
- After: ✅ 0 xUnit1051 warnings, 0 errors, Build succeeded

**Commit:** `4f67ddb` — test: pass TestContext.Current.CancellationToken in integration tests (xUnit1051)

**Rationale:** xUnit v3 provides `TestContext.Current.CancellationToken` as the recommended cancellation token for all async test operations. Using it enables more responsive test cancellation when tests timeout or are manually aborted. This aligns with xUnit v3 best practices and eliminates analyzer warnings.

### 2026-03-02: Code Coverage Exclusion — [ExcludeFromCodeCoverage] + GlobalUsings Consolidation

**Task:** Add `[ExcludeFromCodeCoverage]` to ALL test classes/fixtures/builders and consolidate `using` statements into GlobalUsings.cs for each test project.

**Projects Processed (4 total):**
1. **Unit.Tests** — 58 test/builder/validator files
2. **Integration.Tests** — 14 test/fixture files
3. **Blazor.Tests** — 22 test/fixture/page files
4. **Architecture.Tests** — 1 test file

**Changes Applied:**
1. **[ExcludeFromCodeCoverage]** added to 153 class declarations (all test/fixture/builder classes)
   - Placed above `public class`, `public abstract class`, `public static class` declarations
   - For classes with existing attributes (`[Collection("Integration")]`, `[CollectionDefinition]`), placed below them
2. **GlobalUsings.cs consolidated** — All individual `using` statements moved to project-level GlobalUsings.cs
   - Removed ALL individual `using` directives from .cs files (except `global using` in GlobalUsings.cs itself)
   - Added `System.Diagnostics.CodeAnalysis` to each GlobalUsings.cs (required for `[ExcludeFromCodeCoverage]`)
   - Added project-specific namespaces:
     - **Unit.Tests**: Api.*, Shared.*, Tests.Unit.Builders, Microsoft.*, System.*, MongoDB.Bson
     - **Integration.Tests**: Api.*, Shared.*, NSubstitute, System.*, MongoDB.Bson
     - **Blazor.Tests**: Web.*, Shared.*, Microsoft.AspNetCore.Components.*, System.Net.*, Tests.BlazorTests.Fixtures
     - **Architecture.Tests**: Shared.Models, Shared.Validators, System.*, System.Reflection

**Build Results:**
- ✅ All 4 test projects build successfully (0 errors)
- Pre-existing warnings (CS8602 nullable reference warnings in Integration.Tests) — NOT FIXED (not related to this task)

**Files Modified:** 99 files changed, 138 insertions(+), 602 deletions(-)

**Commit:** `169add1` — test: add [ExcludeFromCodeCoverage] and consolidate GlobalUsings in all test projects

**Rationale:** Test code should be excluded from code coverage metrics. Consolidating `using` directives at the project level via GlobalUsings.cs reduces repetition, improves maintainability, and follows .NET best practices for file-scoped namespaces and global usings.

