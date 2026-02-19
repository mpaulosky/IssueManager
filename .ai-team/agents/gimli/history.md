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

