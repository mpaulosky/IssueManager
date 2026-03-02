# API Test Coverage Gap Scope

**Date:** 2026-03-01  
**Agent:** Aragorn (Lead Developer)  
**Requested by:** Matthew Paulosky  
**Assigned to:** Gimli (Tester)  

## Summary

Comprehensive scope analysis of API test coverage gaps. ZERO endpoint unit tests exist, integration handler tests missing for 3/4 resources, and integration repository tests missing for 2/4 resources. Four GitHub issues created and assigned to Gimli for Sprint backlog.

---

## Gaps Identified

### 1. **Endpoint Unit Tests (CRITICAL GAP)**

**Problem:** No unit tests exist for any endpoint registration files.

**Files Affected:**
- `src/Api/Handlers/Issues/IssueEndpoints.cs` — 5 endpoints (List, Get, Create, Update, Delete)
- `src/Api/Handlers/Categories/CategoryEndpoints.cs` — 5 endpoints
- `src/Api/Handlers/Comments/CommentEndpoints.cs` — 5 endpoints (with issueId filtering)
- `src/Api/Handlers/Statuses/StatusEndpoints.cs` — 5 endpoints

**Total test coverage gap:** 20 endpoints untested

**What needs testing:**
- Route binding (path parameters, query strings)
- HTTP status codes (200, 201, 204, 400, 404, 401)
- Authorization enforcement on POST/PATCH/DELETE
- Handler invocation with correct command/query objects
- Error handling and validation error responses

**Pattern reference:** Use NSubstitute mocks per existing handler tests (`CreateIssueHandlerTests.cs`)

---

### 2. **Integration Handler Tests (HIGH GAP)**

**Problem:** Integration tests only exist for Issues handlers (8 files). Categories, Comments, and Statuses handlers have ZERO integration test coverage.

**Missing Test Files:**

**Categories:**
- `CreateCategoryHandlerIntegrationTests`
- `UpdateCategoryHandlerIntegrationTests`
- `DeleteCategoryHandlerIntegrationTests`
- `ListCategoriesHandlerIntegrationTests`

**Comments:**
- `CreateCommentHandlerIntegrationTests`
- `UpdateCommentHandlerIntegrationTests`
- `DeleteCommentHandlerIntegrationTests`
- `ListCommentsHandlerIntegrationTests`

**Statuses:**
- `CreateStatusHandlerIntegrationTests`
- `UpdateStatusHandlerIntegrationTests`
- `DeleteStatusHandlerIntegrationTests`
- `ListStatusesHandlerIntegrationTests`

**Total test coverage gap:** 12 handler integration tests

**Pattern reference:**
- Use `MongoDbContainer` with TestContainers (mongo:latest)
- Implement `IAsyncLifetime` for container lifecycle
- Use `[Collection("Integration")]` attribute per squad decision on 2026-02-24
- Reference existing Issues handler integration tests

---

### 3. **Integration Repository Tests (MEDIUM GAP)**

**Problem:** Repository integration tests only exist for Issues and Categories. Comments and Statuses repositories have ZERO integration test coverage.

**Missing Test Files:**
- `tests/Integration.Tests/Data/CommentRepositoryTests.cs`
  - CreateAsync, GetByIdAsync, UpdateAsync, DeleteAsync, GetAllAsync (with issueId filtering)
- `tests/Integration.Tests/Data/StatusRepositoryTests.cs`
  - CreateAsync, GetByIdAsync, UpdateAsync, DeleteAsync, GetAllAsync

**Total test coverage gap:** 2 repository integration test files

**Pattern reference:** `CategoryRepositoryTests.cs` provides complete CQRS/MongoDB integration pattern

---

### 4. **Program.cs Startup Tests (LOWER PRIORITY)**

**Problem:** `src/Api/Program.cs` contains critical composition code with zero test coverage:
- Auth0 authentication registration
- Authorization policy setup
- CORS configuration
- OpenAPI/Scalar setup
- API versioning configuration
- Dependency injection composition (repositories, validators, handlers, CurrentUserService)
- Middleware pipeline order
- Endpoint mapping

**Scope:** Verify all extensions execute without errors and dependencies are properly registered.

---

## GitHub Issues Created

| # | Title | Assigned | Priority |
|---|-------|----------|----------|
| 63 | Add endpoint unit tests for Issues, Categories, Comments, Statuses | Gimli | Critical |
| 64 | Add integration handler tests for Categories, Comments, and Statuses | Gimli | High |
| 65 | Add integration repository tests for CommentRepository and StatusRepository | Gimli | Medium |
| 66 | Add Program.cs startup/composition tests | Gimli | Lower |

All issues labeled: `squad`, `squad:gimli`

---

## Test Architecture Decisions

### Pattern Consistency
- **Unit tests:** Mock all dependencies with NSubstitute
- **Integration tests:** Use TestContainers.MongoDB (mongo:latest image)
- **Collection attribute:** Required on all integration test classes per 2026-02-24 decision
- **CancellationToken:** Use `TestContext.Current.CancellationToken` for xUnit v3
- **Test lifecycle:** IAsyncLifetime with ValueTask for container management

### Test Data Strategy
- Use existing builder pattern (`IssueBuilder`, `CategoryBuilder`, etc.)
- Extend builders if needed for new resources (CommentBuilder, StatusBuilder)
- Use `UserDto.Empty`, `CategoryDto.Empty` for nested object defaults

### Scope Boundaries
- **Gimli owns:** All four issues (unit, integration handlers, integration repositories, startup tests)
- **Aragorn owns:** Architecture review, PR gating
- **Sam owns:** Bug fixes or hotfixes discovered during test authoring
- **Legolas owns:** Any integration with Web frontend

---

## Verification Checklist

- [ ] Gimli picks up Issue 63 (endpoint tests)
- [ ] Gimli picks up Issue 64 (integration handler tests)
- [ ] Gimli picks up Issue 65 (integration repository tests)
- [ ] Gimli picks up Issue 66 (Program.cs tests) — lower priority
- [ ] All issues linked to squad member assignment via `squad:gimli` label
- [ ] Test files follow naming convention: `{Resource}Tests.cs` (unit), `{Resource}RepositoryTests.cs` (integration)
- [ ] All tests pass build-repair gate before merge
- [ ] Aragorn reviews PRs for architecture alignment

---

## Impact

**Current state:**
- 20 endpoints: 0% test coverage
- 12 integration handlers: 0% test coverage
- 2 repositories: 0% test coverage
- 1 startup composition: 0% test coverage

**Target state after completion:**
- 100% endpoint unit test coverage (20 tests)
- 100% integration handler test coverage (12 tests)
- 100% integration repository test coverage (2 tests)
- Full Program.cs composition verification

**Risk mitigation:** Closing these gaps prevents silent failures in CI/CD and ensures API robustness.

---

**Aragorn signature:** Lead Developer  
**Status:** Scoped, Issues created, Assigned to Gimli  
**Next action:** Gimli reviews issues and begins Issue #63
