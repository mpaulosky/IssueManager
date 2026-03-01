# Decision: Phase 3 Test Warning Cleanup

**Date:** 2026-02-28
**Author:** Gimli (Tester)
**Status:** Complete
**Commit:** `414828f`

## What

Eliminated all pre-push hook compiler warnings from `tests/Unit.Tests/` and `tests/Blazor.Tests/`:

### CS0618 — Bunit.TestContext obsolete (7 files)

Migrated from `Bunit.TestContext` → `BunitContext` (the non-obsolete bUnit 2.x class). Affected:
- `tests/Blazor.Tests/Fixtures/ComponentTestBase.cs`
- `tests/Blazor.Tests/Pages/Categories/CategoriesPageTests.cs`
- `tests/Blazor.Tests/Pages/Categories/CreateCategoryPageTests.cs`
- `tests/Blazor.Tests/Pages/Categories/EditCategoryPageTests.cs`
- `tests/Blazor.Tests/Pages/Statuses/StatusesPageTests.cs`
- `tests/Blazor.Tests/Pages/Statuses/CreateStatusPageTests.cs`
- `tests/Blazor.Tests/Pages/Statuses/EditStatusPageTests.cs`

### xUnit1051 — CancellationToken.None at ~50 call sites (10 files)

Replaced `CancellationToken.None` / omitted-CT with `Xunit.TestContext.Current.CancellationToken`. Affected:
- `tests/Unit.Tests/Handlers/Categories/ListCategoriesHandlerTests.cs`
- `tests/Unit.Tests/Handlers/Statuses/ListStatusesHandlerTests.cs`
- `tests/Unit.Tests/Repositories/RepositoryValidationTests.cs`
- `tests/Blazor.Tests/Services/StatusApiClientTests.cs`
- `tests/Blazor.Tests/Services/CategoryApiClientTests.cs`
- `tests/Blazor.Tests/Services/CommentApiClientTests.cs`
- `tests/Blazor.Tests/Services/IssueApiClientTests.cs`

## Patterns Established

### BunitContext migration pattern
```csharp
// Before (CS0618)
private readonly Bunit.TestContext _ctx;
_ctx = new Bunit.TestContext();

// After
private readonly BunitContext _ctx;
_ctx = new BunitContext();
```

### CancellationToken fix — handler tests with NSubstitute
```csharp
// Before (xUnit1051)
_repository.GetAllAsync().Returns(Result<...>.Ok(data));
var result = await _handler.Handle(CancellationToken.None);
await _repository.Received(1).GetAllAsync();

// After
_repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result<...>.Ok(data));
var result = await _handler.Handle(Xunit.TestContext.Current.CancellationToken);
await _repository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
```
Use `Arg.Any<CancellationToken>()` in NSubstitute setups — handlers may or may not forward the CT.

### CancellationToken fix — API client tests
```csharp
// Before (xUnit1051 - omitting optional CT parameter)
var result = await client.GetAllAsync();
var result = await client.CreateAsync(command);
var result = await client.DeleteAsync(id);

// After
var result = await client.GetAllAsync(Xunit.TestContext.Current.CancellationToken);
var result = await client.CreateAsync(command, Xunit.TestContext.Current.CancellationToken);
var result = await client.DeleteAsync(id, Xunit.TestContext.Current.CancellationToken);

// Special case: CommentApiClient.GetAllAsync(string? issueId, CancellationToken) — use named param
var result = await client.GetAllAsync(cancellationToken: Xunit.TestContext.Current.CancellationToken);
```

## Production Bug Flagged (NOT fixed — out of scope)

`ListCategoriesHandler.Handle(CancellationToken cancellationToken = default)` accepts a CT but calls `_repository.GetAllAsync()` **without forwarding** the token. This means cancellation requests are silently ignored for list-categories operations. `ListStatusesHandler` correctly forwards the CT. Recommend Aragorn/Sam fix `ListCategoriesHandler` to pass `cancellationToken` to `GetAllAsync()`.

## Results

- Unit.Tests: **390/390 passed**, 0 warnings, 0 errors
- Blazor.Tests: **143/143 passed**, 0 warnings, 0 errors
- Pre-push gate: all 3 suites (Unit, Blazor, Architecture) ✅
- Pushed to `origin/main`
