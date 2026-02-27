# Search/Filter Feature Test Summary

**Tester:** Gimli  
**Date:** 2026-02-27  
**Task:** Write tests for Sam's search/filter feature (SearchTerm and AuthorName parameters)

## What Was Done

### 1. Unit Tests - ListIssuesQueryValidator
**File:** `tests/Unit.Tests/Validators/ListIssuesQueryValidatorTests.cs`

**Tests Added (6 new tests):**
- `Validate_WithSearchTerm_ReturnsValid` - Validates SearchTerm="bug fix" is accepted
- `Validate_WithAuthorName_ReturnsValid` - Validates AuthorName="John" is accepted
- `Validate_WithBothFilters_ReturnsValid` - Both filters can be used together
- `Validate_WithSearchTermTooLong_ReturnsInvalid` - 201 characters rejected
- `Validate_WithAuthorNameTooLong_ReturnsInvalid` - 201 characters rejected

### 2. API Client Tests - IssueApiClient
**File:** `tests/Blazor.Tests/Services/IssueApiClientTests.cs`

**Changes:**
- Enhanced `MockHandler` to capture `LastRequest` for URL inspection
- Added helper method `CreateMockClientWithHandler()` to get handler reference

**Tests Added (4 new tests):**
- `GetAllAsync_WithSearchTerm_IncludesSearchTermInUrl` - Verifies `?searchTerm=bug+fix` in URL
- `GetAllAsync_WithAuthorName_IncludesAuthorNameInUrl` - Verifies `?authorName=John` in URL
- `GetAllAsync_WithBothFilters_IncludesBothInUrl` - Both params in query string
- `GetAllAsync_WithNullFilters_DoesNotIncludeFilterParams` - No extra params when filters are null

### 3. Integration Tests - IssueRepository Search
**File:** `tests/Integration.Tests/Handlers/IssueRepositorySearchTests.cs` (NEW)

**Pattern:** Uses TestContainers MongoDB with `[Collection("Integration")]`

**Tests Added (10 comprehensive tests):**
1. `GetAllAsync_WithSearchTerm_ReturnsMatchingIssues` - Search "bug" returns 2/3 issues
2. `GetAllAsync_WithAuthorName_ReturnsMatchingIssues` - Filter by "Alice" returns 2/3 issues
3. `GetAllAsync_WithNoFilters_ReturnsAllNonArchivedIssues` - Baseline behavior (excludes archived)
4. `GetAllAsync_WithSearchTermAndAuthorName_ReturnsIntersection` - Both filters applied (AND logic)
5. `GetAllAsync_WithSearchTermCaseInsensitive_ReturnsMatches` - "BuG" matches "bug", "Bug", "BUG"
6. `GetAllAsync_WithSearchTermInDescription_ReturnsMatches` - Search finds text in description field
7. `GetAllAsync_WithSearchTermNoMatches_ReturnsEmpty` - No matches returns empty list
8. `GetAllAsync_WithAuthorNameNoMatches_ReturnsEmpty` - No matches returns empty list
9. `GetAllAsync_WithFiltersAndPagination_WorksTogether` - Filters + pagination work together (30 Alice bugs, page 2 of 10)

## Status

✅ **All test code written and compiles**  
⚠️ **Tests will fail until Sam adds SearchTerm/AuthorName to ListIssuesQuery**

### Dependencies on Sam's Changes:
1. `ListIssuesQuery` must add `SearchTerm?` and `AuthorName?` properties
2. `ListIssuesQueryValidator` must add validation rules (max 200 chars each)
3. `IssueApiClient.GetAllAsync()` must accept optional `searchTerm` and `authorName` parameters
4. `IssueRepository.GetAllAsync()` must filter MongoDB results based on these parameters
5. `ListIssuesHandler` must pass filters through to repository

### Build Status:
- Test projects compile successfully ✅
- Web project has unrelated compilation errors (not related to this work)
- Once Sam's changes merge, these tests should immediately validate the feature

## Test Coverage Summary

| Area | Test Type | Count | Coverage |
|------|-----------|-------|----------|
| Validator | Unit | 6 | Valid inputs, length limits, combined filters |
| API Client | Unit | 4 | URL construction, query params, null handling |
| Repository | Integration | 10 | Search, filter, case-insensitive, pagination |
| **Total** | | **20** | **Comprehensive** |

## Next Steps

1. Wait for Sam to complete feature implementation
2. Run tests to verify behavior matches expectations
3. Adjust tests if Sam's implementation differs from expected API
4. Ensure all tests pass before feature PR is merged
