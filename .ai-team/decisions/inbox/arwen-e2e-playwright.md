# E2E Testing Strategy with Playwright

**Date:** 2026-02-17  
**Author:** Arwen (Frontend Dev)  
**Status:** Proposed  

## Context

IssueManager requires end-to-end tests to validate complete user workflows in a realistic browser environment. These tests complement existing unit, integration, and bUnit tests by verifying the application works correctly from a user's perspective.

## Decision

Implemented comprehensive E2E testing with Playwright for .NET covering 30 test scenarios across 6 critical workflow areas.

## Approach

### 1. Technology Choice: Playwright

**Why Playwright:**
- Official .NET support with async/await patterns
- Fast, reliable browser automation
- Supports multiple browsers (Chromium, Firefox, WebKit)
- Headless mode for CI/CD integration
- Active maintenance and Microsoft backing

**Alternatives Considered:**
- **Selenium:** More verbose API, slower, less reliable
- **Puppeteer:** Limited .NET support
- **Cypress:** JavaScript-only, not native .NET integration

### 2. Page Object Model (POM)

**Pattern:** Encapsulate page interactions in dedicated classes
- `HomePage` — Home page navigation
- `IssueFormPage` — Issue creation/editing
- `IssueListPage` — List, filtering, searching
- `IssueDetailPage` — Detail view, status updates

**Benefits:**
- Reduces code duplication across tests
- Centralizes selector management (easier updates)
- Improves test readability and maintainability
- Reusable methods across test suites

### 3. Test Organization

**6 Test Suites:**
1. **IssueCreationTests** (8 tests) — Form submission, validation, status selection
2. **IssueListTests** (6 tests) — List display, filtering, searching, navigation
3. **IssueDetailTests** (4 tests) — Detail view, metadata, edit navigation
4. **IssueStatusUpdateTests** (3 tests) — Status changes, issue updates
5. **NavigationTests** (4 tests) — User flows across pages
6. **ErrorHandlingTests** (5 tests) — Validation errors, 404s, recovery

**Design Principles:**
- Each test is independent (no shared state)
- Tests use unique timestamps to avoid conflicts
- Declarative naming ("User_CanCreateIssueWithValidData")
- Both happy paths and error scenarios covered

### 4. Test Isolation & Data Management

**Strategy:** No shared test database or cleanup between tests

**Implementation:**
- Each test creates its own test data with unique identifiers (timestamp-based)
- Tests tolerate existing data (filters by unique identifiers)
- No explicit teardown/cleanup (tests don't interfere)

**Trade-offs:**
- **Pro:** Tests run in parallel without conflicts
- **Pro:** Simple, no complex setup/teardown
- **Con:** Test data accumulates in database
- **Mitigation:** Use dev/test environment, periodic cleanup

### 5. Configuration & Environment

**Base URL:** Configurable via environment variable (`E2E_BASE_URL`)
- Default: `http://localhost:5000`
- CI/CD: Override with staging/test environment URL

**Browser Configuration:**
- **Default:** Chromium, headless, 1920x1080 viewport
- **Customizable:** Modify `PlaywrightFixture` for debugging (headed mode)

### 6. CI/CD Integration

**Design for CI:**
- Headless mode by default
- Fast execution (~5 seconds per test, ~2.5 minutes total)
- Clear failure messages with assertions
- No external dependencies (except running app)

**Future:** Screenshot capture on failure, video recording

## Impact

### For the Team

- **Gandalf (Lead):** E2E tests provide confidence in architecture and integration
- **Aragorn (Backend):** API contract validation through UI workflows
- **Gimli (Tester):** Comprehensive test coverage at the highest level
- **Legolas (DevOps):** CI/CD pipeline can run E2E tests automatically

### Coverage

- **30 E2E tests** covering all critical user workflows
- **100% coverage** of primary user journeys (create, list, detail, update, navigate)
- **Error scenarios** tested (validation, 404s, concurrent submissions)

### Maintenance

- Page Object Model makes tests easy to update when UI changes
- Clear test structure and naming for readability
- Documented in `tests/E2E/README.md` with setup instructions

## Alternatives Not Chosen

### 1. Selenium WebDriver
- **Reason:** More verbose API, slower, less reliable than Playwright

### 2. Blazor Server Circuit Testing
- **Reason:** Not a true E2E test (doesn't test in real browser)
- **Use Case:** Better suited for integration tests with bUnit

### 3. Manual Testing Only
- **Reason:** Not scalable, error-prone, no CI/CD automation

## Risks & Mitigations

### Risk: Flaky Tests
- **Mitigation:** Use explicit waits (`WaitForURLAsync`, `IsVisibleAsync`) instead of delays
- **Mitigation:** Avoid timing dependencies, test in headless mode

### Risk: Test Data Accumulation
- **Mitigation:** Use dev/test environment with periodic cleanup
- **Future:** Implement cleanup fixture or database reset

### Risk: Playwright Browser Installation
- **Mitigation:** Documented in README, simple command (`playwright install`)
- **CI/CD:** Add to pipeline setup steps

## Follow-up Work

1. **CI/CD Integration:** Add E2E tests to GitHub Actions or Azure Pipelines
2. **Screenshot Capture:** Implement automatic screenshots on test failure
3. **Cross-Browser Testing:** Extend to Firefox and WebKit
4. **Parallel Execution:** Configure xUnit for parallel test execution
5. **Accessibility Testing:** Add tests for ARIA roles, keyboard navigation

## References

- [Playwright for .NET Documentation](https://playwright.dev/dotnet/)
- [Page Object Model Pattern](https://martinfowler.com/bliki/PageObject.html)
- [xUnit Async Lifetime](https://xunit.net/docs/shared-context#async-lifetime)

---

**Decision:** Approved by Arwen  
**Feedback:** Awaiting team review (Gandalf, Aragorn, Gimli, Legolas)
