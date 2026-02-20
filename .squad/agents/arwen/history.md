# History — Arwen

## Project Learnings (from init)

### IssueManager Project — Started 2026-02-17

**Tech Stack:**
- Blazor (server-side or WebAssembly, TBD)
- .NET 10.0
- Bootstrap or other CSS framework (TBD)
- API integration with Aragorn's backend

**Blazor patterns to follow:**
- Components are reusable, composable units
- Use cascading parameters for parent-child data flow
- Isolate component logic — keep pages thin
- Async operations for API calls

**UI flows:**
- Issue list: display, sort, filter, search
- Issue detail: read, edit, delete
- Issue create: form with validation
- User feedback: loading states, error messages, success confirmations

---

## Learnings

### E2E Testing with Playwright (2026-02-17)

**Playwright Setup & Architecture:**
- Created comprehensive E2E test suite with 30 tests covering all critical user workflows
- Implemented Page Object Model (POM) pattern for maintainability and reusability
- Page objects encapsulate page interactions: `HomePage`, `IssueFormPage`, `IssueListPage`, `IssueDetailPage`
- `PlaywrightFixture` manages browser lifecycle using xUnit's `IAsyncLifetime` pattern
- Browser configuration: Chromium, headless by default, 1920x1080 viewport
- Base URL configurable via environment variable (`E2E_BASE_URL`) for flexibility across environments

**Async Patterns:**
- All Playwright operations are async — strict use of `async`/`await` throughout
- Used explicit waits (`WaitForURLAsync`, `IsVisibleAsync`) instead of arbitrary delays
- Fixture implements proper async disposal (`DisposeAsync`) to clean up browser resources

**Test Organization:**
- 6 test suites organized by workflow: Creation (8), List (6), Detail (4), Status Update (3), Navigation (4), Error Handling (5)
- Tests are declarative — read like user stories ("User can create issue with valid data")
- Each test is independent, uses unique timestamps to avoid data conflicts
- Theory tests with `InlineData` for parameterized scenarios (e.g., testing all status values)

**Blazor Integration in Browser:**
- Playwright interacts with Blazor components via standard DOM selectors (CSS, text content)
- Blazor's interactive server rendering works seamlessly with Playwright
- Form validation errors are visible in the DOM and testable with `IsVisibleAsync`
- Component lifecycle (loading states, spinners) can be tested by checking element visibility
- Navigation between Blazor pages triggers URL changes detectable with `WaitForURLAsync`

**Challenges & Solutions:**
- **Browser installation:** Required explicit Playwright browser installation step documented in README
- **Timing issues:** Addressed with explicit waits rather than sleep/delays for reliable tests
- **Test isolation:** Used timestamp-based unique identifiers to prevent test interference
- **Error scenarios:** Tested both happy paths and error cases (validation, 404s, concurrent submissions)

**Key Insights:**
- Page Object Model dramatically reduces code duplication across tests
- Explicit waits are essential for flaky-free E2E tests with Blazor's dynamic rendering
- Testing validation requires checking both field-level errors and validation summaries
- Navigation tests verify the full user journey (list → create → detail → list)
- Error recovery tests ensure users can fix validation errors and successfully resubmit

*Append new UI patterns, Blazor insights, and integration notes here as you work.*
