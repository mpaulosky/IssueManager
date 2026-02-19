# Testing Strategy

This document outlines the testing philosophy, strategy, and quality gates for IssueManager.

## Philosophy

**Why we test:** Quality gates prevent regressions, document behavior, and give confidence during refactoring. Tests are living documentation that show how components work and fail.

**Coverage goals:** 
- **80%+ for handlers and validators** (business logic)
- **60%+ for Blazor components** (UI interactions)
- **100% for architecture rules** (design constraints)
- **Critical paths covered** by integration and E2E tests

**Test independently:** Each test should run in isolation without depending on other tests. Use fixtures and factories to create test data.

## Test Pyramid

We follow the standard test pyramid with four layers:

```
     /\     E2E Tests (~15 tests)
    /  \    ↓ Slow, high coverage, critical user workflows
   /____\   Integration Tests (~17 tests)
  /      \  ↓ Vertical slices, MongoDB, handlers + validators
 /________\ Unit Tests (~30 tests) + Architecture Tests (~10 tests)
            ↓ Fast, focused, one thing per test
```

### Unit Tests (~30 tests)
- **What:** Validators, domain models, pure logic
- **Frameworks:** xUnit, FluentAssertions, FluentValidation
- **Speed:** <100ms per test
- **When to write:** For every Command/Query validator, domain model, service method
- **Example:** `CreateIssueValidatorTests.cs`

### Architecture Tests (~10 tests)
- **What:** Layer boundaries, naming conventions, dependency rules
- **Framework:** NetArchTest.Rules
- **Speed:** ~500ms per test
- **When to write:** When adding architectural constraints (e.g., "Validators cannot depend on Web layer")
- **Example:** `ArchitectureTests.cs`

### Integration Tests (~17 tests)
- **What:** Handlers with real MongoDB, full vertical slices (validator → handler → repository)
- **Frameworks:** xUnit, TestContainers, MongoDB
- **Speed:** ~2-5s per test (container startup amortized)
- **When to write:** For each Command/Query handler to verify persistence and database integration
- **Example:** `CreateIssueHandlerTests.cs`

### Blazor Component Tests (~13 tests)
- **What:** Blazor component rendering, parameter binding, event callbacks
- **Framework:** bUnit
- **Speed:** ~200ms per test
- **When to write:** For reusable components with complex logic (forms, data tables)
- **Example:** `IssueFormTests.cs`

### E2E Tests (~31 tests)
- **What:** Complete user workflows in a real browser (Chromium)
- **Framework:** Playwright for .NET
- **Speed:** ~5s per test
- **When to write:** For critical user journeys (create issue, list/filter, detail view, status updates)
- **Example:** `IssueCreationTests.cs`
- **Setup:** Requires Playwright browsers installed (`playwright install chromium`)
- **Environment:** Application must be running (e.g., `dotnet run` from AppHost)

### E2E Tests (~15 tests)
- **What:** Browser automation, critical user workflows (create issue, list issues, etc.)
- **Framework:** Playwright
- **Speed:** ~10-30s per test
- **When to write:** For primary user journeys and critical workflows
- **Example:** Issue creation flow, issue list filtering

### Blazor Component Tests (~13 tests)
- **What:** Component rendering, parameters, event callbacks, forms
- **Framework:** bUnit
- **Speed:** <500ms per test
- **When to write:** For reusable components, forms, and complex UI logic
- **Example:** `IssueFormTests.cs`

## When to Write Which Type of Test

| Scenario | Test Type | Why |
|----------|-----------|-----|
| Validator rules (required, min/max length) | Unit | Fast, focused, no dependencies |
| Domain model behavior (e.g., Issue.AddLabel) | Unit | Pure logic, no I/O |
| Handler creates and persists Issue | Integration | Verify full slice with real database |
| Component renders form correctly | Blazor (bUnit) | UI-specific, parameters and callbacks |
| User creates an issue via browser | E2E | End-to-end workflow, real user interaction |
| Domain layer must not depend on MongoDB | Architecture | Design constraint enforcement |

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Suite
```bash
# Unit tests
dotnet test tests/Unit

# Integration tests
dotnet test tests/Integration

# Blazor component tests
dotnet test tests/BlazorTests

# Architecture tests
dotnet test tests/Architecture

# E2E tests (requires app running)
dotnet test tests/E2E
```

### Run Tests with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run Tests in Watch Mode
```bash
dotnet watch test --project tests/Unit
```

## Quality Gates

### Before Submitting a PR
- ✅ All tests pass locally (`dotnet test`)
- ✅ No new warnings or errors
- ✅ New features include tests (unit + integration)
- ✅ Bug fixes include regression tests
- ✅ Coverage meets targets (80%+ handlers, 60%+ components)
- ✅ Tests are fast (<5s for unit/arch, <30s for integration)
- ✅ No flaky tests (tests pass 10/10 times)

### PR Review Checklist
- [ ] Tests exist for new code
- [ ] Tests are clear and maintainable
- [ ] Tests follow naming conventions (descriptive names)
- [ ] Test data is isolated (no shared state)
- [ ] Assertions are specific (not just `Should().BeTrue()`)
- [ ] No commented-out tests

## Test Structure

All tests follow **Arrange-Act-Assert** (AAA) or **Given-When-Then** patterns:

```csharp
[Fact]
public void Validator_EmptyTitle_ReturnsValidationError()
{
    // Arrange — Set up test data and dependencies
    var validator = new CreateIssueValidator();
    var command = new CreateIssueCommand { Title = "" };

    // Act — Execute the code under test
    var result = validator.Validate(command);

    // Assert — Verify the outcome
    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.PropertyName == "Title");
}
```

## Test Naming Conventions

Use descriptive names that document behavior:

```
[MethodUnderTest]_[Scenario]_[ExpectedOutcome]
```

Examples:
- ✅ `Handle_ValidCommand_StoresIssueInDatabase`
- ✅ `IssueForm_ShowsUpdateButtonText_WhenIsEditModeIsTrue`
- ✅ `CreateIssueValidator_EmptyTitle_ReturnsValidationError`
- ❌ `Test1` (too vague)
- ❌ `TestValidation` (what about validation?)

## Performance Tuning

### Fast Tests
- **Keep unit tests under 100ms:** No I/O, no sleeps
- **Use TestContainers efficiently:** Share container fixtures when possible
- **Parallel execution:** xUnit runs test classes in parallel by default
- **Avoid Thread.Sleep:** Use async/await or test timeouts

### Avoiding Flaky Tests
- **Don't depend on timing:** Use `await` or `Task.WhenAny` instead of fixed delays
- **Isolate test data:** Each test should create its own data (unique IDs, separate collections)
- **Clean up resources:** Use `IAsyncLifetime` or `IDisposable` to tear down containers/state
- **Mock external dependencies:** Never call real APIs or third-party services in tests

## Debugging Failed Tests

### Unit Test Failures
1. Read the assertion message (FluentAssertions provides detailed diffs)
2. Set a breakpoint in the test
3. Run test in Debug mode (`dotnet test --filter "FullyQualifiedName~YourTest"`)
4. Inspect variables and step through logic

### Integration Test Failures
1. Check MongoDB container logs (`docker logs <container_id>`)
2. Verify connection string is correct
3. Add debug logging to handler/repository
4. Check test isolation (is another test interfering?)

### E2E Test Failures
1. Run test in headed mode (see browser)
2. Check Playwright trace logs
3. Add screenshots on failure
4. Verify app is running and accessible

### Architecture Test Failures
1. Read the assertion message (shows which types violate the rule)
2. Review dependency graph (is the violation intentional?)
3. Update architecture rule or fix the code

## CI/CD Integration

Tests run automatically on every PR and push to `main`:

- **Unit & Architecture Tests:** Run first (fast feedback)
- **Integration Tests:** Run after unit tests pass (MongoDB via TestContainers)
- **E2E Tests:** Run last (requires app deployment)
- **Coverage Report:** Generated and uploaded to codecov.io

## Common Mistakes

❌ **Testing too many things in one test**
- Split into focused tests

❌ **Using production data or shared state**
- Create isolated test data per test

❌ **Not cleaning up resources**
- Use `IDisposable` or `IAsyncLifetime`

❌ **Brittle assertions** (e.g., exact string matches)
- Use flexible matchers like `.Should().Contain()`

❌ **Testing implementation details**
- Focus on observable behavior, not internal state

## Test Guides

For detailed guides on each test type, see:

- **[Unit Testing Guide](guides/UNIT-TESTS.md)** — xUnit, FluentValidation, FluentAssertions
- **[Blazor Component Testing Guide](guides/BUNIT-BLAZOR-TESTS.md)** — bUnit, component lifecycle
- **[Architecture Testing Guide](guides/ARCHITECTURE-TESTS.md)** — NetArchTest, design rules
- **[Integration Testing Guide](guides/INTEGRATION-TESTS.md)** — TestContainers, MongoDB, handlers
- **[E2E Testing Guide](guides/E2E-PLAYWRIGHT-TESTS.md)** — Playwright, browser automation
- **[Test Data & Fixtures Guide](guides/TEST-DATA.md)** — Builders, factories, isolation

## Further Reading

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [bUnit Documentation](https://bunit.dev/)
- [TestContainers .NET](https://testcontainers.com/)
- [Playwright .NET](https://playwright.dev/dotnet/)
- [NetArchTest](https://github.com/BenMorris/NetArchTest)
