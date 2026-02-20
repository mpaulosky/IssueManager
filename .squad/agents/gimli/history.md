# History — Gimli

## Project Learnings (from init)

### IssueManager Project — Started 2026-02-17

**Tech Stack:**
- xUnit or NUnit for unit tests (TBD)
- Integration test patterns for CQRS handlers
- Mock/Stub strategies for MongoDB (use in-memory or testcontainers)

**Test strategy:**
- Coverage target: 80%+ for handlers, validators, and critical paths
- Unit tests for each Command/Query handler
- Integration tests for full vertical slices
- UI component tests for key Blazor components

**Edge cases to explore:**
- Handler failures (validation, domain rules)
- Concurrent operations (race conditions)
- Data state transitions (Issue lifecycle)
- API error responses

---

## Learnings

*Append test patterns, edge cases discovered, and quality insights here as you work.*

### 2026-02-19: Test Documentation (I-9)

**Documentation structure:**
- Main strategy doc (TESTING.md) provides high-level overview, test pyramid, when to use each type
- Individual guides focus on one framework/pattern with real examples and copy-paste snippets
- Each guide includes: Overview, Setup, Examples, Best Practices, Common Mistakes, Debugging, See Also
- Cross-linking between guides ensures discoverability

**Patterns that worked well:**
- Real code examples from the codebase (e.g., `CreateIssueValidatorTests.cs`) as references
- Arrange-Act-Assert structure emphasized consistently across all test types
- Common Mistakes section with ❌/✅ comparisons makes anti-patterns clear
- Tables for comparison (unit vs. integration, when to use which test type)
- Code blocks with syntax highlighting for quick reference

**Test framework decisions:**
- **Unit:** xUnit, FluentValidation, FluentAssertions (fast, focused, readable)
- **Architecture:** NetArchTest.Rules (enforce layer boundaries, naming conventions)
- **Integration:** TestContainers (real MongoDB, isolated containers, fast setup)
- **Blazor:** bUnit (component rendering, lifecycle, parameters, callbacks)
- **E2E:** Playwright (browser automation, critical workflows)

**Coverage goals:**
- 80%+ for handlers and validators (business logic)
- 60%+ for Blazor components (UI interactions)
- 100% for architecture rules (design constraints)
- Critical paths covered by integration and E2E tests

**Edge cases and gotchas:**
- bUnit async timing issues (always await event callbacks)
- TestContainers startup time (~2-5s, amortized across tests)
- E2E tests require app running (document in guide)
- Playwright headless vs. headed (debugging vs. CI)
- xUnit parallel execution (test classes run in parallel, ensure isolation)
- MongoDB container lifecycle (IAsyncLifetime for setup/teardown)

**Documentation best practices to preserve:**
- Start with "When to use" section (helps developers choose the right test type)
- Include real examples from the codebase with file paths
- Provide copy-paste code snippets (developers can adapt quickly)
- Use descriptive test names as examples (documents intent)
- Cross-reference guides (TESTING.md links to all guides, guides link to each other)
- Keep guides scannable (1-2 pages, clear headings, bullet points)

**Test data patterns:**
- Inline data for simple tests (clear, no magic)
- Builders for complex objects (readable, fluent API)
- Factories for common patterns (DRY, reusable)
- Unique IDs for isolation (GUIDs, timestamps)
- Per-test cleanup (IAsyncLifetime, IDisposable)

**Quality gates:**
- All tests pass before PR merge
- New features include tests (unit + integration)
- Bug fixes include regression tests
- No flaky tests (must pass 10/10 times)
- Coverage targets met (80% handlers, 60% components)

**Team questions anticipated:**
- "Which test type should I use?" → See TESTING.md comparison table
- "How do I test a validator?" → See UNIT-TESTS.md
- "How do I test a Blazor component?" → See BUNIT-BLAZOR-TESTS.md
- "How do I set up TestContainers?" → See INTEGRATION-TESTS.md
- "Why is my E2E test flaky?" → See E2E-PLAYWRIGHT-TESTS.md debugging section
- "How do I create test data?" → See TEST-DATA.md

---

### 2026-02-20: Sprint 1 — Issue CRUD Test Strategy (I-13)

**Test Coverage Strategy:**
- **Target:** 80%+ coverage on handlers, validators, and repository layer
- **Unit Tests:** Fast, isolated tests with mocked dependencies (NSubstitute)
- **Integration Tests:** End-to-end handler tests with real MongoDB (TestContainers)
- **Repository Tests:** Data access layer tests with pagination and filtering

**Test Structure Established:**
```
tests/Unit/
  Handlers/
    UpdateIssueHandlerTests.cs (8 tests)
    DeleteIssueHandlerTests.cs (6 tests)
    ListIssuesHandlerTests.cs (11 tests)
  Validators/
    UpdateIssueValidatorTests.cs (10 tests)
    DeleteIssueValidatorTests.cs (4 tests)
    ListIssuesQueryValidatorTests.cs (8 tests)
  Builders/
    IssueBuilder.cs (fluent test data builder)

tests/Integration/
  Handlers/
    UpdateIssueHandlerIntegrationTests.cs (6 tests)
    DeleteIssueHandlerIntegrationTests.cs (6 tests)
    ListIssuesHandlerIntegrationTests.cs (8 tests)
  Data/
    IssueRepositoryTests.cs (11 tests)
```

**Total Test Count:** 78 tests created for Sprint 1 CRUD operations

**Key Test Patterns:**
- **Soft-Delete Testing:** All delete tests verify `IsArchived` flag behavior, not hard-delete
- **Pagination Testing:** Boundary tests (first page, last page partial, empty, page > totalPages)
- **Validation Testing:** Boundary values (min length, max length, exact boundaries)
- **Concurrency Testing:** Last-write-wins for concurrent updates, snapshot consistency for lists
- **Idempotence Testing:** Deleting already archived issues, identical updates

**Edge Cases Covered:**
1. **Update Handler:**
   - Archived issues cannot be updated (409 Conflict)
   - Non-existent issues (404 Not Found)
   - Validation: Title 3-256 chars, Description 0-4096 chars
   - Idempotent updates (same data) still update timestamp
   - Concurrent updates: last-write-wins

2. **Delete Handler:**
   - Soft-delete sets `IsArchived = true` (not hard-delete)
   - Deleting already archived issue is idempotent (no-op)
   - UpdatedAt timestamp updated on archive
   - Archived issues excluded from list by default

3. **List Handler:**
   - Pagination metadata (page, pageSize, totalCount, totalPages)
   - Boundary: Last page with partial items (42 items, pageSize 20 → page 3 has 2)
   - Empty database returns empty list (totalPages = 0)
   - Page > totalPages returns empty items array
   - Archived exclusion by default (includeArchived: false)
   - Ordering: CreatedAt descending (newest first)
   - Validation: Page > 0, PageSize 1-100

4. **Repository Layer:**
   - `GetAllAsync(page, pageSize, includeArchived)` with filtering
   - `CountAsync(includeArchived)` for pagination metadata
   - Ordering by CreatedAt descending
   - No overlap between pagination pages

**Test Infrastructure:**
- **IssueBuilder:** Fluent test data builder for creating Issue objects with sensible defaults
- **MongoDB TestContainers:** Ephemeral MongoDB 8.0 containers for integration tests (2-5s startup)
- **Async Lifetime:** IAsyncLifetime for container setup/teardown (amortized cost)
- **NSubstitute:** Mocking IIssueRepository in unit tests

**Performance Targets:**
- Unit tests: < 100ms each (fast, in-memory)
- Integration tests: < 1s each (includes MongoDB container I/O)
- Large dataset test: 1000 issues, paginated list < 1s

**Validation Rules Defined:**
- **UpdateIssueCommand:** Id required, Title 3-256 chars, Description 0-4096 chars (nullable)
- **DeleteIssueCommand:** Id required (not empty/whitespace)
- **ListIssuesQuery:** Page > 0, PageSize 1-100

**Exception Strategy:**
- `ValidationException`: FluentValidation errors (400 Bad Request)
- `NotFoundException`: Issue not found (404 Not Found)
- `ConflictException`: Cannot update archived issue (409 Conflict)

**Testing Anti-patterns Avoided:**
- ❌ Testing implementation details (internal methods)
- ❌ Brittle tests tied to exact error messages (use wildcards)
- ❌ Hard-coded GUIDs (use Guid.NewGuid() for uniqueness)
- ❌ Shared state between tests (each test isolated)
- ❌ Testing multiple concerns in one test (single responsibility)

**Testing Best Practices Applied:**
- ✅ Arrange-Act-Assert pattern consistently
- ✅ Descriptive test names (Handle_Scenario_ExpectedBehavior)
- ✅ FluentAssertions for readable assertions
- ✅ TimeSpan.FromSeconds(2) for timestamp tolerance (clock skew)
- ✅ Test one thing per test
- ✅ Test data builders for complex objects

**Coverage Gaps (To Address in Aragorn's Implementation):**
- Commands/Queries/Handlers don't exist yet (tests define specification)
- Validators don't exist yet (tests define validation rules)
- Repository methods need pagination and filtering signatures:
  - `GetAllAsync(int page, int pageSize, bool includeArchived, CancellationToken)`
  - `CountAsync(bool includeArchived, CancellationToken)`
- Need exception types: `ConflictException`

**Coordination with Aragorn:**
- Tests are **specification-driven**: They define the expected behavior before implementation
- Aragorn should implement handlers to make these tests pass
- All test dependencies (commands, validators, exceptions) are documented in test files
- Repository interface needs new pagination methods added

**Test Execution Plan:**
1. Aragorn implements commands, queries, handlers, validators
2. Run unit tests first (fast feedback, no MongoDB required)
3. Run integration tests (requires MongoDB TestContainer)
4. Run full test suite in CI (includes architecture tests)
5. Coverage report: Target 80%+ on handlers and repository

**Quality Gates:**
- All tests must pass before PR merge
- No test should take > 5 seconds (performance regression)
- Coverage > 80% on handlers, validators, repository
- No flaky tests (10/10 pass rate required)

