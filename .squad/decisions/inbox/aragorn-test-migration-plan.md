# Test Migration Plan: Domain Model Alignment

**Status:** Plan Created (Awaiting Gimli execution)  
**Created By:** Aragorn (Lead Developer)  
**Affected Systems:** Unit, Integration, Blazor Tests (130+ test files)  

## Executive Summary

The Issue entity and related domain types have undergone significant refactoring. Tests were written against old API contracts and now fail to compile. This plan categorizes the failures into 7 logical groups and defines the fix strategy.

---

## Root Cause Analysis

### Current Domain Model State

**Issue Entity** (Shared.Models.Issue):
- Constructor signature: `Issue(ObjectId id, string title, string description, DateTime dateCreated, UserDto author, CategoryDto category, StatusDto status)`
- Properties: `Id` (ObjectId), `Title`, `Description`, `DateCreated`, `DateModified` (nullable), `Category`, `Author`, `IssueStatus`, `Archived`, `ArchivedBy`, `ApprovedForRelease`, `Rejected`
- **Removed**: `AuthorId`, `IsArchived` (now `Archived`), `CategoryId`, `StatusId`, `DateCreated` is now init-only

**IssueStatus Enum**: Does not exist as an enum. Status is now a `Status` class (Shared.Models.Status) with `StatusName` and `StatusDescription` properties.

**IIssueRepository Interface** (Api.Data):
- `GetAllAsync()` returns `IReadOnlyList<IssueDto>`
- `GetAllAsync(page, pageSize)` returns tuple `(IReadOnlyList<IssueDto>, long)`
- **Changed**: No `includeArchived` parameter anywhere
- **Changed**: `CountAsync()` takes only `CancellationToken`

**Result<T> Pattern** (Shared.Abstractions.Result):
- Located in `Shared.Abstractions` namespace (not in test context)
- Tests using bare `Result` type need explicit namespacing or imports

**Exception Types** (Shared.Exceptions):
- `NotFoundException(string message)` exists
- `ConflictException(string message)` exists
- Tests missing `using Shared.Exceptions;`

**DateTimeAssertions API** (FluentAssertions):
- `NotBeNull()` no longer exists on `DateTimeAssertions`
- Datetime assertions now return `AndConstraint<DateTimeAssertions>` for chaining
- Tests accessing `.Value` on non-nullable DateTime are incorrect

---

## Failure Categories and Fix Strategy

### Group 1: Entity Constructor Parameters (~15 files)
**Error Pattern**: `CS1739: 'Issue' does not have a parameter named 'AuthorId'`

**Tests Affected**:
- `tests/Unit.Tests/Builders/IssueBuilder.cs` (lines 139-147)
- `tests/Unit.Tests/Handlers/UpdateIssueHandlerTests.cs` (multiple)
- `tests/Unit.Tests/Handlers/DeleteIssueHandlerTests.cs` (multiple)
- `tests/Unit.Tests/Handlers/ListIssuesHandlerTests.cs` (line 173)
- Integration test handlers and builders

**Fix Strategy**:
1. Update all `new Issue(authorId: ...)` calls to remove `AuthorId` parameter
2. Pass `UserDto.Empty` as `author` parameter instead
3. Remove `CategoryId`, `StatusId` parameters; pass `CategoryDto.Empty`, `StatusDto.Empty`
4. Fix constructor signature calls to match: `new Issue(id, title, description, dateCreated, author, category, status)`

**Priority**: HIGH (blocks most tests)

---

### Group 2: IsArchived → Archived Property Renaming (~10 files)
**Error Pattern**: `CS0117: 'Issue' does not contain a definition for 'IsArchived'`

**Tests Affected**:
- Delete/Update handler tests
- Repository integration tests
- All assertions checking archived status

**Fix Strategy**:
1. Rename all `.IsArchived` property access to `.Archived`
2. Example: `issue.IsArchived = true` → `issue.Archived = true`
3. Example: `x.IsArchived.Should().BeTrue()` → `x.Archived.Should().BeTrue()`

**Priority**: HIGH (blocks execution paths)

---

### Group 3: IssueStatus Enum → Status Class References (~8 files)
**Error Pattern**: `CS0103: The name 'IssueStatus' does not exist in the current context`

**Tests Affected**:
- `tests/Integration.Tests/Handlers/UpdateIssueStatusHandlerTests.cs` (lines 52, 60, 65, 75, 92, 110, 116, 122, 128, 133)
- `tests/Integration.Tests/Handlers/CreateIssueHandlerTests.cs` (line 62)

**Fix Strategy**:
1. Replace `IssueStatus.Open` → `StatusDto.Empty` (or appropriate `StatusDto` constant)
2. Replace enum value construction with `new StatusDto(statusName, statusDescription)`
3. Example: `status = new StatusDto("Open", "Issue is open")`
4. Add `using Shared.Models;` and `using Shared.DTOs;` to affected files

**Priority**: MEDIUM (integration tests only)

---

### Group 4: Result<T> Namespace Missing (~5 files)
**Error Pattern**: `CS0103: The name 'Result' does not exist in the current context`

**Tests Affected**:
- `tests/Unit.Tests/Handlers/ListIssuesHandlerTests.cs` (lines 33, 54, 75, 95, 115)

**Fix Strategy**:
1. Add `using Shared.Abstractions;` to affected test files
2. Verify Result<T> is used correctly with proper generic type
3. Example: `var result = handler.Handle(query);` returns `Result<PaginatedResponse<IssueResponseDto>>`

**Priority**: MEDIUM (affects query/result validation)

---

### Group 5: Exception Type Imports (~3 files)
**Error Pattern**: `CS0246: The type or namespace name 'NotFoundException' could not be found`

**Tests Affected**:
- Handler tests checking exception throws (Delete, Update handlers)
- Integration tests verifying exception behavior

**Fix Strategy**:
1. Add `using Shared.Exceptions;` to test files
2. Tests already using correct exception types; only imports needed

**Priority**: LOW (straightforward import fix)

---

### Group 6: Handler Constructor Parameter Changes (~4 files)
**Error Pattern**: `CS7036: There is no argument given that corresponds to the required parameter 'validator'`

**Tests Affected**:
- `DeleteIssueHandler` instantiation (Unit + Integration)
- `ListIssuesHandler` instantiation (Integration)

**Current Signatures**:
- `DeleteIssueHandler(IIssueRepository, DeleteIssueValidator)`
- `ListIssuesHandler(IIssueRepository, ListIssuesQueryValidator)`

**Fix Strategy**:
1. Create validator instances when instantiating handlers
2. Example: `new DeleteIssueHandler(_repository, new DeleteIssueValidator())`
3. Verify validator types exist and are constructible without arguments

**Priority**: MEDIUM (blocks handler setup)

---

### Group 7: DateTimeAssertions API Changes + PaginatedResponse Property (~5 files)
**Error Pattern**: `CS1061: 'DateTimeAssertions' does not contain a definition for 'NotBeNull'`
**Error Pattern**: `CS1061: 'PaginatedResponse<IssueResponseDto>' does not contain a definition for 'TotalCount'`

**Tests Affected**:
- Update handler assertions (DateModified null checks)
- List handler integration tests (pagination metadata)

**Fix Strategy**:
1. Replace `x.DateModified?.NotBeNull()` with `x.DateModified.Should().NotBeNull()`
2. Replace `.Value` access on non-nullable DateTime with direct property access
3. Replace `result.TotalCount` with `result.Total` (tuple-based API)
4. Example: `result.TotalCount.Should().Be(42)` → `var (items, total) = result; total.Should().Be(42);`

**Priority**: MEDIUM (assertion refinement)

---

## Implementation Order

**Phase 1 (Blocking Dependencies):**
1. Group 1 - Entity Constructor Parameters
2. Group 2 - IsArchived → Archived

**Phase 2 (Independent Fixes):**
3. Group 3 - IssueStatus → StatusDto
4. Group 4 - Result<T> Namespace
5. Group 5 - Exception Imports

**Phase 3 (Dependent on Phase 1):**
6. Group 6 - Handler Constructor Updates
7. Group 7 - DateTimeAssertions & PaginatedResponse

---

## File Summary by Category

### Unit Tests (tests/Unit.Tests/)
- **Builders**: `IssueBuilder.cs` (Groups 1, 2)
- **Handlers**: `ListIssuesHandlerTests.cs`, `UpdateIssueHandlerTests.cs`, `DeleteIssueHandlerTests.cs`, `UpdateIssueStatusHandlerTests.cs`
  - Issues: Groups 1, 2, 4, 5, 6, 7

### Integration Tests (tests/Integration.Tests/)
- **Data**: `IssueRepositoryTests.cs` (Groups 1, 2)
- **Handlers**: Multiple handler integration tests
  - Issues: Groups 1, 2, 3, 5, 6, 7

### Blazor Tests (tests/BlazorTests/)
- Likely has similar patterns

---

## Acceptance Criteria

✓ All test projects compile without CS errors  
✓ All test projects compile without unresolved reference warnings  
✓ All existing tests pass (functional validation)  
✓ No logic changes to domain model during fix  
✓ All 7 fix groups applied to relevant files  

---

## Notes for Gimli (Tester)

- Use this plan as a checklist; work through groups sequentially
- Phase 1 is prerequisite for Phase 3 execution
- Each group contains multiple files; batch-apply fixes per group for efficiency
- Validate by running `dotnet build` after each phase
- Example: After Group 1, run build to verify constructor fixes; then proceed to Group 2

---

**Next Step**: Route to Gimli for execution. Target: All tests compile and pass.
