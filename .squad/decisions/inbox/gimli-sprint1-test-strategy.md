# Sprint 1 Test Strategy — Issue CRUD Coverage

**By:** Gimli (Tester)  
**Date:** 2026-02-20  
**Issue:** I-13  
**Status:** Ready for Review by Gandalf

---

## Decision Summary

Established comprehensive test coverage strategy for Sprint 1 Issue CRUD operations (Update, Delete/Archive, List with pagination). Created 78 tests across unit, integration, and repository layers targeting 80%+ coverage on critical paths.

---

## Test Coverage Goals

### Coverage Targets
- **Handlers:** 80%+ line coverage
- **Validators:** 80%+ line coverage  
- **Repository:** 80%+ line coverage
- **Overall Critical Paths:** 80%+ coverage

### Test Distribution
- **Unit Tests (47 tests):** Handlers (25), Validators (22)
- **Integration Tests (20 tests):** Handlers (20)
- **Repository Tests (11 tests):** Data access layer

---

## Test Architecture

### Unit Tests (Fast, Isolated)
- **Location:** `tests/Unit/Handlers/`, `tests/Unit/Validators/`
- **Dependencies:** NSubstitute for mocking IIssueRepository
- **Execution:** < 100ms per test (in-memory, no I/O)
- **Purpose:** Test business logic in isolation

**Handler Tests:**
- `UpdateIssueHandlerTests` (8 tests): Happy path, validation errors, not found, archived conflict, idempotence
- `DeleteIssueHandlerTests` (6 tests): Soft-delete, not found, idempotence, timestamp updates
- `ListIssuesHandlerTests` (11 tests): Pagination, boundaries, empty lists, archived exclusion, ordering

**Validator Tests:**
- `UpdateIssueValidatorTests` (10 tests): Id, title, description validation (boundaries)
- `DeleteIssueValidatorTests` (4 tests): Id validation (required, not empty/whitespace)
- `ListIssuesQueryValidatorTests` (8 tests): Page/PageSize validation (boundaries 1-100)

### Integration Tests (Real Database)
- **Location:** `tests/Integration/Handlers/`, `tests/Integration/Data/`
- **Dependencies:** TestContainers.MongoDB (mongo:8.0)
- **Execution:** < 1s per test (includes MongoDB I/O)
- **Purpose:** Test full vertical slices with real database

**Handler Integration Tests:**
- `UpdateIssueHandlerIntegrationTests` (6 tests): Full update flow, timestamp updates, atomic operations, concurrency
- `DeleteIssueHandlerIntegrationTests` (6 tests): Soft-delete persists, exclusion from lists, idempotence
- `ListIssuesHandlerIntegrationTests` (8 tests): Pagination with real data, archived exclusion, ordering, performance

**Repository Integration Tests:**
- `IssueRepositoryTests` (11 tests): Pagination, filtering, soft-delete, ordering, count operations

---

## Key Test Patterns

### 1. Soft-Delete Pattern
All delete operations test `IsArchived` flag behavior:
- Set `IsArchived = true` (not hard-delete)
- Update `UpdatedAt` timestamp on archive
- Exclude archived issues from list by default (`includeArchived: false`)
- Idempotent: Deleting already archived issue is no-op

### 2. Pagination Testing
Comprehensive boundary testing for paginated lists:
- First page (page=1, pageSize=20)
- Second page (page=2, ensure no overlap)
- Last page partial (42 items, pageSize=20 → page 3 has 2 items)
- Empty database (totalPages = 0)
- Page > totalPages (returns empty items array)
- Validation: Page > 0, PageSize 1-100

### 3. Validation Boundary Testing
Test exact boundaries for all validators:
- **Title:** Min 3 chars, Max 256 chars (test 2, 3, 256, 257)
- **Description:** Max 4096 chars (nullable, test 4096, 4097)
- **Page:** Min 1 (test 0, -1, 1)
- **PageSize:** Min 1, Max 100 (test 0, 1, 100, 101)

### 4. Concurrency Testing
- **Update Handler:** Last-write-wins (concurrent updates → latest timestamp wins)
- **List Handler:** Snapshot consistency (list stable during concurrent creates)

### 5. Idempotence Testing
- **Update:** Identical updates still update timestamp
- **Delete:** Deleting already archived issue succeeds without changes

---

## Edge Cases Covered

### Update Handler Edge Cases
- ✅ Archived issues cannot be updated (409 Conflict)
- ✅ Non-existent issues return 404 Not Found
- ✅ Empty/null title validation (400 Bad Request)
- ✅ Title/description length validation (3-256, 0-4096)
- ✅ Idempotent updates (same data → timestamp still updated)
- ✅ Concurrent updates (last-write-wins)

### Delete Handler Edge Cases
- ✅ Soft-delete sets IsArchived = true
- ✅ Non-existent issue returns 404 Not Found
- ✅ Already archived issue is idempotent (no-op)
- ✅ UpdatedAt timestamp updated on archive
- ✅ Record persists in database (soft-delete, not hard-delete)

### List Handler Edge Cases
- ✅ Empty database returns empty list (totalPages = 0)
- ✅ Page > totalPages returns empty items array
- ✅ Last page with partial items (correct count)
- ✅ Archived issues excluded by default
- ✅ Ordering: CreatedAt descending (newest first)
- ✅ Large dataset performance (1000 issues < 1s)

---

## Test Infrastructure

### IssueBuilder (Fluent Test Data Builder)
Created reusable builder for Issue test data:
```csharp
var issue = IssueBuilder.Default()
    .WithTitle("Test Issue")
    .WithDescription("Test Description")
    .AsArchived()
    .Build();
```

**Benefits:**
- Sensible defaults (no boilerplate in tests)
- Fluent API (readable test setup)
- Consistent test data across suite

### MongoDB TestContainers
- **Image:** mongo:8.0
- **Startup:** ~2-5s (amortized with IAsyncLifetime)
- **Isolation:** Each test class gets fresh container
- **Cleanup:** Automatic container disposal after tests

### Mocking Strategy (Unit Tests)
- **Library:** NSubstitute
- **Pattern:** Mock IIssueRepository in handler tests
- **Verification:** `Received(1)` for interaction verification

---

## Exception Strategy

### Exception Types Defined
- **ValidationException:** FluentValidation errors → 400 Bad Request
- **NotFoundException:** Issue not found → 404 Not Found
- **ConflictException:** Cannot update archived issue → 409 Conflict

### Error Handling Tests
- All validators test error messages (wildcards for flexibility)
- All handlers test exception paths (not found, conflict, validation)
- Integration tests verify exceptions propagate correctly

---

## Performance Targets

### Unit Tests
- **Target:** < 100ms per test
- **Rationale:** In-memory, no I/O (fast feedback)

### Integration Tests
- **Target:** < 1s per test
- **Rationale:** Includes MongoDB container I/O

### Large Dataset Test
- **Scenario:** 1000 issues, paginated list (page=1, pageSize=20)
- **Target:** < 1s
- **Purpose:** Ensure pagination scales

---

## Coverage Gaps (For Aragorn)

### Missing Implementation (Tests Define Specification)
1. **Commands:**
   - `UpdateIssueCommand` (Id, Title, Description)
   - `DeleteIssueCommand` (Id)
   - `ListIssuesQuery` (Page, PageSize)

2. **Handlers:**
   - `UpdateIssueHandler`
   - `DeleteIssueHandler`
   - `ListIssuesHandler`

3. **Validators:**
   - `UpdateIssueValidator`
   - `DeleteIssueValidator`
   - `ListIssuesQueryValidator`

4. **Repository Methods (Extend IIssueRepository):**
   ```csharp
   Task<IReadOnlyList<Issue>> GetAllAsync(int page, int pageSize, bool includeArchived, CancellationToken);
   Task<long> CountAsync(bool includeArchived, CancellationToken);
   ```

5. **Exception Types:**
   - `ConflictException` (for archived issue updates)

---

## Coordination with Aragorn

### Test-Driven Development Flow
1. **Gimli (Me):** Created 78 tests defining expected behavior
2. **Aragorn:** Implements handlers, commands, validators to make tests pass
3. **Validation:** Run tests continuously (red → green → refactor)
4. **Coverage Report:** Verify 80%+ coverage achieved

### Testability Requirements
- Handlers accept dependencies via constructor (DI-friendly)
- Repository interface extended with pagination methods
- Commands/Queries are simple DTOs (easy to test)
- Validators are isolated (FluentValidation rules)

---

## Quality Gates

### Pre-Merge Checklist
- ✅ All 78 tests pass (100% pass rate)
- ✅ No test takes > 5 seconds (performance regression check)
- ✅ Coverage > 80% on handlers, validators, repository
- ✅ No flaky tests (10/10 consecutive passes required)

### CI Integration
- Unit tests run first (fast feedback)
- Integration tests run with MongoDB TestContainer
- Coverage report generated (Coverlet)
- Architecture tests included (NetArchTest.Rules)

---

## Testing Best Practices Applied

### Code Quality
- ✅ Arrange-Act-Assert pattern (consistent structure)
- ✅ Descriptive test names (Handle_Scenario_ExpectedBehavior)
- ✅ FluentAssertions (readable, discoverable assertions)
- ✅ One concern per test (single responsibility)

### Maintainability
- ✅ Test data builders (DRY, consistent fixtures)
- ✅ Shared MongoDB fixture (amortize startup cost)
- ✅ TimeSpan tolerance for timestamps (clock skew resilience)
- ✅ Wildcards in error message assertions (not brittle)

### Anti-patterns Avoided
- ❌ Testing implementation details (test behavior, not internals)
- ❌ Brittle exact error message checks (use wildcards)
- ❌ Hard-coded GUIDs (use Guid.NewGuid() for uniqueness)
- ❌ Shared state between tests (each test isolated)

---

## Test Execution Strategy

### Local Development
```bash
# Run all unit tests (fast)
dotnet test --filter "FullyQualifiedName~Tests.Unit"

# Run all integration tests (slower)
dotnet test --filter "FullyQualifiedName~Tests.Integration"

# Run specific handler tests
dotnet test --filter "FullyQualifiedName~UpdateIssueHandlerTests"

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
```

### CI Pipeline
1. Unit tests (parallel execution)
2. Integration tests (sequential, requires MongoDB)
3. Architecture tests (layer boundaries, naming conventions)
4. Coverage report (publish to CI artifacts)
5. Quality gate: 80% coverage + 100% pass rate

---

## Next Steps

1. **Aragorn:** Review test specifications, implement handlers/commands/validators
2. **Gimli:** Support implementation with test execution guidance
3. **Gandalf:** Review test strategy, approve or request changes
4. **Team:** Merge after all tests pass and coverage targets met

---

## Files Created

### Unit Tests
- `tests/Unit/Handlers/UpdateIssueHandlerTests.cs` (8 tests)
- `tests/Unit/Handlers/DeleteIssueHandlerTests.cs` (6 tests)
- `tests/Unit/Handlers/ListIssuesHandlerTests.cs` (11 tests)
- `tests/Unit/Validators/UpdateIssueValidatorTests.cs` (10 tests)
- `tests/Unit/Validators/DeleteIssueValidatorTests.cs` (4 tests)
- `tests/Unit/Validators/ListIssuesQueryValidatorTests.cs` (8 tests)
- `tests/Unit/Builders/IssueBuilder.cs` (fluent builder)

### Integration Tests
- `tests/Integration/Handlers/UpdateIssueHandlerIntegrationTests.cs` (6 tests)
- `tests/Integration/Handlers/DeleteIssueHandlerIntegrationTests.cs` (6 tests)
- `tests/Integration/Handlers/ListIssuesHandlerIntegrationTests.cs` (8 tests)
- `tests/Integration/Data/IssueRepositoryTests.cs` (11 tests)

**Total:** 11 files, 78 tests

---

## Approval Request

This test strategy is ready for Gandalf's review. Once approved, Aragorn can implement the handlers with confidence that comprehensive test coverage is in place.

**Questions for Review:**
1. Is 80% coverage target appropriate for Sprint 1?
2. Should we add any additional edge cases?
3. Should validation rules be adjusted (title 3-256, description 0-4096)?
4. Should delete idempotence return 204 (no-op) or 404 (not found)?

---

**Signed:** Gimli, Tester  
**Status:** Awaiting Gandalf's Approval
