# Integration Test Strategy for IssueManager

**Author:** Aragorn (Backend/Data Engineer)  
**Date:** February 19, 2026  
**Status:** ✅ Implemented

---

## Context

Integration tests are essential to verify **end-to-end vertical slices** of the application—from API handlers through validators to repository operations and database persistence. These tests ensure that the entire flow works correctly with real infrastructure (MongoDB) rather than mocks or in-memory fakes.

---

## Decision

Implemented **17 integration tests** using **TestContainers for MongoDB 8.0** to test the complete vertical slice architecture:

### Architecture

```
Integration Test
  ├─ TestContainers MongoDB (real database, ephemeral)
  ├─ Handler (CQRS command/query pattern)
  ├─ Validator (FluentValidation)
  └─ Repository (MongoDB persistence layer)
```

### Test Organization

1. **Handler Tests (3 test classes, 17 tests total)**:
   - `CreateIssueHandlerTests.cs` - 8 tests
   - `GetIssueHandlerTests.cs` - 5 tests
   - `UpdateIssueStatusHandlerTests.cs` - 4 tests

2. **Test Infrastructure**:
   - `MongoDbFixture.cs` - Shared TestContainer setup
   - `GlobalUsings.cs` - Centralized imports

### Test Coverage

#### CreateIssueHandler (8 tests)
- ✅ Valid command stores issue in database
- ✅ Valid command with labels stores issue with labels
- ✅ Empty title throws validation exception
- ✅ Title too short throws validation exception
- ✅ Title too long throws validation exception
- ✅ Multiple issues all persisted correctly
- ✅ Valid command with null description creates issue
- ✅ Created issue has correct timestamps

#### GetIssueHandler (5 tests)
- ✅ Existing issue ID returns issue
- ✅ Non-existing issue ID returns null
- ✅ Empty issue ID throws argument exception
- ✅ Get all with multiple issues returns all issues
- ✅ Get all with empty database returns empty list

#### UpdateIssueStatusHandler (4 tests)
- ✅ Valid command updates issue status
- ✅ Non-existing issue returns null
- ✅ Empty issue ID throws validation exception
- ✅ Status transition (Open→InProgress→Closed) updates correctly

---

## Implementation Details

### TestContainers Configuration

- **Image:** `mongo:8.0`
- **Container Lifecycle:** `IAsyncLifetime` (per-test-class isolation)
- **Startup Time:** ~4 seconds per container
- **Total Test Time:** ~48.5 seconds for 17 tests
- **Cleanup:** Automatic container disposal after tests

### Database Isolation Strategy

Each test class creates its own ephemeral MongoDB container:
- Tests within a class share the same container
- Each test has a clean database state (no data pollution)
- Containers are automatically stopped and removed after tests

### Handler Infrastructure Created

1. **Repository Layer**:
   - `IIssueRepository` (interface)
   - `IssueRepository` (MongoDB implementation)
   - MongoDB entity mapping (`IssueEntity`, `LabelEntity`)

2. **Handler Layer**:
   - `CreateIssueHandler` - Creates new issues with validation
   - `GetIssueHandler` - Retrieves issues by ID or all issues
   - `UpdateIssueStatusHandler` - Updates issue status with validation

3. **Validators** (already existed):
   - `CreateIssueValidator` - Title/description/labels validation
   - `UpdateIssueStatusValidator` - Issue ID and status validation

---

## Test Results

```
Test Run Successful.
Total tests: 17
     Passed: 17
     Failed: 0
   Skipped: 0
 Total time: 48.5 seconds
```

**MongoDB Container Startup Time:** ~4 seconds (average)

---

## Benefits

1. **Real Database Testing**: Uses actual MongoDB instead of in-memory fakes
2. **Vertical Slice Coverage**: Tests entire flow from handler to database
3. **Validator Integration**: Ensures validators work correctly with handlers
4. **Data Consistency**: Verifies persistence and retrieval operations
5. **Transaction Boundaries**: Tests CRUD operations with real transactions
6. **Isolation**: Each test class has its own container (no data pollution)
7. **CI/CD Ready**: TestContainers works in CI environments with Docker

---

## Trade-offs

| Aspect | Choice | Rationale |
|--------|--------|-----------|
| Container Per Class | ✅ Selected | Balances speed vs isolation |
| Container Per Test | ❌ Rejected | Too slow (~4s startup × 17 tests = 68s) |
| Shared Container | ❌ Rejected | Data pollution between tests |
| In-Memory Fake | ❌ Rejected | Doesn't test MongoDB-specific behavior |

---

## Future Enhancements

1. **Add more handler tests** as new features are implemented:
   - `UpdateIssueHandler` (update title/description)
   - `DeleteIssueHandler` (soft delete)
   - `SearchIssuesHandler` (filtering/pagination)

2. **Add transaction tests** when implementing:
   - Multi-document operations
   - Rollback scenarios

3. **Add performance tests** for:
   - Bulk operations (create 1000 issues)
   - Query performance (pagination)
   - Index effectiveness

4. **Add concurrency tests** for:
   - Concurrent updates
   - Optimistic locking

---

## Files Created

### Handler Infrastructure
- `src/Api/Data/IIssueRepository.cs`
- `src/Api/Data/IssueRepository.cs`
- `src/Api/Handlers/CreateIssueHandler.cs`
- `src/Api/Handlers/GetIssueHandler.cs`
- `src/Api/Handlers/UpdateIssueStatusHandler.cs`

### Integration Tests
- `tests/Integration/GlobalUsings.cs`
- `tests/Integration/Fixtures/MongoDbFixture.cs`
- `tests/Integration/Handlers/CreateIssueHandlerTests.cs` (8 tests)
- `tests/Integration/Handlers/GetIssueHandlerTests.cs` (5 tests)
- `tests/Integration/Handlers/UpdateIssueStatusHandlerTests.cs` (4 tests)

### Project Updates
- Updated `src/Api/Api.csproj` to reference Shared project

---

## Verification

```bash
cd E:\github\IssueManager
dotnet test tests\Integration\Integration.csproj
```

**Result:** ✅ All 17 tests passing

---

## References

- [TestContainers for .NET](https://dotnet.testcontainers.org/)
- [MongoDB TestContainers](https://dotnet.testcontainers.org/modules/mongodb/)
- [xUnit IAsyncLifetime](https://xunit.net/docs/shared-context#async-lifetime)
- [FluentValidation](https://docs.fluentvalidation.net/)
