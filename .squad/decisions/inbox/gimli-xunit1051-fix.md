# Decision: xUnit1051 CancellationToken Integration Test Fix

**Date:** 2026-03-01  
**Author:** Gimli (Tester)  
**Status:** Completed  
**Commit:** `4f67ddb`

## What

Fixed all xUnit1051 analyzer warnings across 10 Integration.Tests files by passing `TestContext.Current.CancellationToken` to every async repository and handler method call within test methods.

## Scope

**Files Fixed (10):**
- `tests/Integration.Tests/Data/CategoryRepositoryTests.cs`
- `tests/Integration.Tests/Data/IssueRepositoryTests.cs`
- `tests/Integration.Tests/Handlers/CreateIssueHandlerTests.cs`
- `tests/Integration.Tests/Handlers/DeleteIssueHandlerIntegrationTests.cs`
- `tests/Integration.Tests/Handlers/DeleteIssueHandlerTests.cs`
- `tests/Integration.Tests/Handlers/GetIssueHandlerTests.cs`
- `tests/Integration.Tests/Handlers/IssueRepositorySearchTests.cs`
- `tests/Integration.Tests/Handlers/ListIssuesHandlerIntegrationTests.cs`
- `tests/Integration.Tests/Handlers/UpdateIssueHandlerIntegrationTests.cs`
- `tests/Integration.Tests/Handlers/UpdateIssueStatusHandlerTests.cs`

**Total Fixes:** 131 async call sites updated

## Pattern Examples

### Repository Method Calls
```csharp
// Before
var result = await _repository.CreateAsync(category);
var result = await _repository.GetByIdAsync(id);
var result = await _repository.GetAllAsync();
var result = await _repository.UpdateAsync(entity);
var result = await _repository.ArchiveAsync(id);

// After
var result = await _repository.CreateAsync(category, TestContext.Current.CancellationToken);
var result = await _repository.GetByIdAsync(id, TestContext.Current.CancellationToken);
var result = await _repository.GetAllAsync(TestContext.Current.CancellationToken);
var result = await _repository.UpdateAsync(entity, TestContext.Current.CancellationToken);
var result = await _repository.ArchiveAsync(id, TestContext.Current.CancellationToken);
```

### Pagination with Optional Parameters
```csharp
// Before
var result = await _repository.GetAllAsync(page: 1, pageSize: 20);

// After (named parameter required due to optional searchTerm/authorName params)
var result = await _repository.GetAllAsync(page: 1, pageSize: 20, cancellationToken: TestContext.Current.CancellationToken);
```

### Handler Calls
```csharp
// Before
var result = await _handler.Handle(command);

// After
var result = await _handler.Handle(command, TestContext.Current.CancellationToken);
```

### Exception Assertions
```csharp
// Before
await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command));

// After
await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, TestContext.Current.CancellationToken));
```

### Task.Delay
```csharp
// Before
await Task.Delay(100);

// After
await Task.Delay(100, TestContext.Current.CancellationToken);
```

## Intentionally NOT Fixed

**Lifecycle Hooks (`InitializeAsync`, `DisposeAsync`):**
- Container lifecycle operations (`_mongoContainer.StartAsync()`, `StopAsync()`, `DisposeAsync()`) were left unchanged.
- **Reason:** `TestContext.Current` is null in xUnit lifecycle hooks. xUnit1051 rule does NOT apply to lifecycle methods.

## Build Results

- **Before:** 103+ xUnit1051 warnings, 0 errors
- **After:** ✅ 0 xUnit1051 warnings, 0 errors
- **Final:** Build succeeded, 46 total warnings (unrelated to xUnit1051)

## Why

xUnit v3 introduced `TestContext.Current.CancellationToken` as the recommended cancellation token for all async test operations. Using it provides:

1. **Responsive Test Cancellation:** When a test times out or is manually aborted, async operations cooperatively cancel via the token.
2. **xUnit v3 Best Practice:** Aligns with analyzer guidance and official xUnit v3 patterns.
3. **Clean Build:** Eliminates all xUnit1051 warnings from Integration.Tests.

## Lessons Learned

### Regex Pitfalls
Automated regex replacement was overly aggressive and initially placed CT parameters incorrectly inside nested method calls:
```csharp
// Incorrect (broken by regex)
await _repository.ArchiveAsync(ObjectId.Parse(id, TestContext.Current.CancellationToken));

// Correct
await _repository.ArchiveAsync(ObjectId.Parse(id), TestContext.Current.CancellationToken);
```
**Fix:** Manual correction required for 2 cases. Lesson: Complex nested calls need manual review after automated replacement.

### Named Parameters for Optional Params
When a method has optional parameters between positional ones and the CT parameter, use named parameter syntax:
```csharp
// Repository signature
GetAllAsync(int page, int pageSize, string? searchTerm = null, string? authorName = null, CancellationToken ct = default)

// Must use named parameter
GetAllAsync(page: 1, pageSize: 20, cancellationToken: TestContext.Current.CancellationToken)
```

### Task.Delay Also Requires CT
`Task.Delay(int millisecondsDelay, CancellationToken ct)` is flagged by xUnit1051. Always pass the test CT:
```csharp
await Task.Delay(100, TestContext.Current.CancellationToken);
```

## Future Work

None required. All xUnit1051 warnings eliminated from Integration.Tests.
