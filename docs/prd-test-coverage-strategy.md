---
title: "xUnit v3 Test Coverage Strategy"
version: "1.0.0"
date: "2026-02-19"
author: "Aragorn (Backend Dev)"
status: "Draft"
---

## Executive Summary

This Product Requirements Document (PRD) defines the comprehensive testing framework and coverage expectations for the IssueManager project. The strategy establishes clear guidelines for unit, integration, Blazor component, architecture, Aspire orchestration, and end-to-end testing using xUnit v3 as the primary testing framework.

**Purpose:** Ensure consistent, high-quality test coverage across all layers of the application while maintaining rapid feedback loops and sustainable test maintenance.

**Scope:** All test categories across the IssueManager vertical slice architecture:
- Unit tests (domain logic, handlers, validators)
- Integration tests (full slices with real MongoDB)
- Architecture tests (layering, dependency rules, naming conventions)
- Blazor component tests (rendering, interaction, state)
- Aspire orchestration tests (app host configuration)
- End-to-end tests (full stack browser automation)

**Success Criteria:**
- 80%+ coverage of business logic (commands, queries, domain validators)
- 100% compliance with architecture rules
- All vertical slices include unit + integration tests before PR merge
- E2E workflows cover 100% of user-facing features
- CI/CD executes all test categories in parallel (~8 minutes total)
- Team adheres to naming, structure, and assertion patterns defined herein

---

## Current State Assessment

### Existing Test Infrastructure

As of 2026-02-18, IssueManager has established test foundations across six projects:

| Project | Type | Tests | Status | Dependencies |
|---------|------|-------|--------|--------------|
| **Unit** | Domain/Handler unit tests | 30 tests | ✅ Building & Passing | xunit, FluentAssertions, FluentValidation, NSubstitute |
| **Integration** | Full-slice tests with MongoDB | 17 tests | ✅ Building & Passing | xunit, FluentAssertions, Testcontainers.MongoDb, MongoDB.Driver |
| **Architecture** | Layering & dependency rules | 10 tests | ✅ Building & Passing | xunit, FluentAssertions, NetArchTest.Rules |
| **BlazorTests** | Component rendering & interaction | 13 tests | ✅ Building & Passing | xunit, bunit, FluentAssertions, NSubstitute |
| **Aspire** | Distributed app orchestration | 0 tests | ⏳ Placeholder structure ready | xunit, FluentAssertions, Aspire.Hosting |
| **E2E** | Browser automation via Playwright | 31 tests | ⚠️ Failing (Playwright setup issue) | xunit, Playwright |

**Total:** 70+ tests actively passing; 31 E2E tests pending environment setup

### What's Working

✅ **Core test infrastructure is stable:**
- All five main test projects build successfully (net10.0, C# 14.0)
- Consistent dependency management via Directory.Packages.props
- GlobalUsings.cs in each project for common imports
- CI/CD pipeline executes all test categories in parallel (squad-test.yml)

✅ **Test patterns are established:**
- xunit for all test runners (console output, attribute-based organization)
- FluentAssertions for readable assertions across all projects
- NSubstitute for mocking dependencies (Unit, BlazorTests)
- Testcontainers for isolated MongoDB testing (Integration)
- bUnit for Blazor component testing (BlazorTests)
- NetArchTest.Rules for architecture compliance (Architecture)

✅ **Domain modeling is clear:**
- Vertical slice architecture with one feature per slice
- CQRS pattern: Commands (state changes), Queries (data retrieval)
- Handlers orchestrate logic; validators enforce rules before execution
- DTOs shield domain entities from the wire

### What's Incomplete

⏳ **E2E tests are not yet running:**
- 31 tests written but failing in CI (Playwright browser binaries missing)
- Requires `dotnet tool install --global Microsoft.Playwright.CLI && playwright install --with-deps chromium`
- Tracked in decisions (PR #14 feedback: fix must go to squad-test.yml test-e2e job)

⏳ **Aspire tests are placeholder:**
- Project structure exists but no tests written
- Will validate app host configuration, service wiring, health checks

⏳ **Coverage baselines not yet established:**
- No formal coverage thresholds defined per category
- No aggregated coverage reporting in CI/CD

⏳ **Test naming/structure conventions not yet documented:**
- Patterns exist organically but need formalization for consistency

### Project Tech Stack Context

- **.NET Version:** 10.0 (latest stable)
- **C# Language:** 14.0 with latest features (records, pattern matching, nullable reference types)
- **Architecture:** Vertical Slice Architecture with CQRS pattern
- **Database:** MongoDB via MongoDB.EntityFramework
- **API Framework:** ASP.NET Core (Minimal APIs)
- **Frontend:** Blazor Server (interactive)
- **Orchestration:** .NET Aspire
- **Package Management:** Centralized in Directory.Packages.props

---

## Test Strategy by Type

### 1. Unit Tests

**When to write:**
- Testing domain validators (FluentValidation rules)
- Testing command handlers (state change logic, pre-conditions)
- Testing query handlers (data transformation, filtering)
- Testing domain services (pure business logic)
- Testing result/error handling in handlers
- Edge cases, boundary conditions, error scenarios

**What to test:**
- **Happy path:** Valid input → Expected output
- **Validation failures:** Invalid input → Appropriate error messages
- **Edge cases:** Null values, empty collections, boundary values
- **Error handling:** Exception handling, fault tolerance, retry logic
- **Side effects:** Mocked dependencies behave as expected

**Expected coverage:** 80%+ of business logic code paths

**Tools & libraries:**
- `xunit` — Test runner and attributes ([Theory], [Fact])
- `FluentAssertions` — Readable assertions (Should().Be(), etc.)
- `FluentValidation` — Domain validators being tested
- `NSubstitute` — Mock external dependencies (repositories, services)

**Patterns:**

```
tests/Unit/
├── Domain/
│   ├── Validators/
│   │   └── IssueValidatorTests.cs
│   └── Models/
│       └── IssueTests.cs
├── Handlers/
│   ├── Commands/
│   │   └── CreateIssueHandlerTests.cs
│   └── Queries/
│       └── GetIssueByIdHandlerTests.cs
└── Services/
    └── IssueServiceTests.cs
```

**Naming convention:** `{SubjectUnderTest}Tests.cs`
- Test method: `{Method}_{Scenario}_{ExpectedResult}()`
- Example: `CreateIssue_WithValidInput_ReturnsNewId()`

**Assertion pattern:**
```csharp
// Arrange
var validator = new IssueValidator();
var issue = new CreateIssueCommand { Title = "" };

// Act
var result = validator.Validate(issue);

// Assert
result.IsValid.Should().BeFalse();
result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateIssueCommand.Title));
```

---

### 2. Integration Tests

**When to write:**
- Testing full vertical slices end-to-end
- Testing handler + repository interaction with real MongoDB
- Testing data persistence and retrieval
- Testing concurrent operations, race conditions
- Testing validation + domain logic + persistence together

**What to test:**
- **Full slice workflow:** Command received → Handler executes → Data persisted → Query retrieves
- **MongoDB operations:** Create, read, update, delete via EF Core
- **Data integrity:** Constraints enforced, relationships maintained
- **Concurrency:** Multiple simultaneous operations
- **Transaction rollback:** Errors don't leave partial state

**Expected coverage:** 60%+ of handler/repository code paths (avoid redundancy with unit tests)

**Tools & libraries:**
- `xunit` — Test runner
- `FluentAssertions` — Readable assertions
- `Testcontainers.MongoDb` — Isolated MongoDB instance per test run
- `MongoDB.EntityFramework` — Real EF Core context
- `MongoDB.Driver` — Direct MongoDB operations (setup/verify)

**Patterns:**

```
tests/Integration/
├── Fixtures/
│   └── MongoDbFixture.cs
├── Handlers/
│   ├── Commands/
│   │   └── CreateIssueHandlerTests.cs
│   └── Queries/
│       └── GetIssuesHandlerTests.cs
└── Repositories/
    └── IssueRepositoryTests.cs
```

**Setup pattern (MongoDbFixture.cs):**
```csharp
public class MongoDbFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _container = new MongoDbBuilder().Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();
    }

    public async Task DisposeAsync() => await _container.StopAsync();
}

[Collection("MongoDB collection")]
public class CreateIssueHandlerTests : IAsyncLifetime
{
    private readonly MongoDbFixture _fixture;
    public async Task InitializeAsync() { /* setup */ }
    public async Task DisposeAsync() { /* cleanup */ }
}
```

**Test method pattern:**
```csharp
[Fact]
public async Task CreateIssue_WithValidCommand_PersistsToDatabase()
{
    // Arrange
    var command = new CreateIssueCommand { Title = "Test Issue", Description = "..." };
    var handler = new CreateIssueHandler(_dbContext);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var persisted = await _dbContext.Issues.FirstOrDefaultAsync(i => i.Id == result.Value);
    persisted.Should().NotBeNull();
    persisted!.Title.Should().Be("Test Issue");
}
```

---

### 3. Architecture Tests

**When to write:**
- On every build (to enforce layering and dependency rules)
- When adding new projects or restructuring code
- When enforcing naming conventions across the codebase
- Verify no circular dependencies exist

**What to test:**
- **Layering rules:** Api layer doesn't reference Web, Domain doesn't reference Api
- **Naming conventions:** All validators end with "Validator", all handlers named "*Handler"
- **Dependency rules:** No external dependencies in Domain layer (except BCL)
- **No circular dependencies:** Project dependency graph is acyclic

**Expected coverage:** 100% compliance (all rules enforced on every build)

**Tools & libraries:**
- `xunit` — Test runner
- `FluentAssertions` — Readable assertions
- `NetArchTest.Rules` — Architecture rule definitions

**Patterns:**

```
tests/Architecture/
├── LayeringTests.cs
├── DependencyTests.cs
├── NamingTests.cs
└── CircularDependencyTests.cs
```

**Example test:**

```csharp
[Fact]
public void Handlers_MustEndWithHandler_Suffix()
{
    var result = Types
        .InAssembly(typeof(CreateIssueCommand).Assembly)
        .That()
        .Inherit(typeof(IRequestHandler<>))
        .Should()
        .HaveNameEndingWith("Handler")
        .GetResult();

    result.IsSuccessful.Should().BeTrue();
}

[Fact]
public void Domain_ShouldNotDependOn_Api()
{
    var result = Types
        .InAssembly(typeof(Issue).Assembly)
        .Should()
        .NotHaveDependencyOn("IssueManager.Api")
        .GetResult();

    result.IsSuccessful.Should().BeTrue();
}
```

---

### 4. Blazor Component Tests (bUnit)

**When to write:**
- Testing component rendering with different parameter combinations
- Testing user interactions (click, input, form submission)
- Testing component lifecycle (OnInitialized, OnParametersSet)
- Testing cascading parameters and render fragments
- Testing error boundaries and error states

**What to test:**
- **Rendering:** Component renders correctly with given parameters
- **Interaction:** User actions trigger event callbacks, update state
- **Lifecycle:** Components initialize, update parameters, clean up correctly
- **Data binding:** Two-way binding works (if applicable)
- **Error states:** Null checks, loading states, error messages display

**Expected coverage:** 60%+ of component code (focus on logic, not trivial markup)

**Tools & libraries:**
- `xunit` — Test runner
- `bunit` — Blazor component testing library
- `FluentAssertions` — Readable assertions
- `NSubstitute` — Mock services injected into components

**Patterns:**

```
tests/BlazorTests/
├── Components/
│   ├── IssueListComponentTests.cs
│   ├── IssueFormComponentTests.cs
│   └── IssueDetailComponentTests.cs
└── Pages/
    └── IssuesPageTests.cs
```

**Test method pattern:**

```csharp
[Fact]
public void IssueListComponent_WithIssues_RendersList()
{
    // Arrange
    using var ctx = new TestContext();
    var issues = new[] { new IssueDto { Id = 1, Title = "Test" } };

    // Act
    var cut = ctx.RenderComponent<IssueListComponent>(
        p => p.Add(c => c.Issues, issues)
    );

    // Assert
    cut.Find("table tbody tr").Should().NotBeNull();
    cut.Find("table tbody tr td:first-child").TextContent.Should().Contain("Test");
}

[Fact]
public async Task IssueFormComponent_OnSubmit_CallsCallback()
{
    // Arrange
    using var ctx = new TestContext();
    var callbackFired = false;
    var command = new CreateIssueCommand();

    // Act
    var cut = ctx.RenderComponent<IssueFormComponent>(
        p => p.Add(c => c.OnSubmit, new EventCallback<CreateIssueCommand>(
            null, new Func<CreateIssueCommand, Task>(async c => {
                callbackFired = true;
                command = c;
                await Task.CompletedTask;
            })
        ))
    );
    
    cut.Find("form").Submit();
    await cut.InvokeAsync(() => { });

    // Assert
    callbackFired.Should().BeTrue();
}
```

---

### 5. Aspire Orchestration Tests

**When to write:**
- Testing app host configuration and service registration
- Verifying all services are wired correctly
- Testing health check endpoints
- Testing service discovery and networking

**What to test:**
- **Service wiring:** All required services are registered
- **Configuration:** Environment variables, settings passed correctly
- **Health checks:** All services expose health check endpoints
- **Resource setup:** Containers, databases start correctly

**Expected coverage:** 100% of app host configuration paths

**Tools & libraries:**
- `xunit` — Test runner
- `FluentAssertions` — Readable assertions
- `Aspire.Hosting` — App host configuration testing

**Patterns:**

```
tests/Aspire/
├── AppHostTests.cs
└── ServiceWiringTests.cs
```

**Test method pattern:**

```csharp
[Fact]
public async Task AppHost_WithAllServices_ConfiguresSuccessfully()
{
    // Arrange
    var builder = new DistributedApplicationBuilder(new DistributedApplicationOptions());
    var apiService = builder.AddProject<Projects.IssueManager_Api>("api");
    var webService = builder.AddProject<Projects.IssueManager_Web>("web");
    var mongoService = builder.AddMongoDB("mongodb");

    // Act
    var app = builder.Build();

    // Assert
    app.Resources.Should().HaveCount(3);
    apiService.Should().NotBeNull();
    webService.Should().NotBeNull();
    mongoService.Should().NotBeNull();
}
```

---

### 6. End-to-End Tests (Playwright)

**When to write:**
- Testing complete user workflows from browser
- Validating full-stack integration (UI → API → Database)
- Testing cross-browser compatibility
- Testing form submission, navigation, user interactions at scale
- Pre-release validation

**What to test:**
- **User workflows:** Create issue → View issue → Edit issue → Delete issue
- **Authentication:** Login, logout, permission-based access
- **Data consistency:** Changes in UI reflected in database
- **Error handling:** Error messages display correctly
- **Performance:** Page load times, response times

**Expected coverage:** 100% of happy-path user workflows (not error scenarios)

**Tools & libraries:**
- `xunit` — Test runner
- `Playwright` — Browser automation (Chromium, Firefox, WebKit)

**Patterns:**

```
tests/E2E/
├── Fixtures/
│   └── BrowserFixture.cs
├── Pages/
│   ├── IssueListPage.cs
│   ├── IssueFormPage.cs
│   └── IssueDetailPage.cs
└── Workflows/
    ├── CreateIssueWorkflowTests.cs
    ├── EditIssueWorkflowTests.cs
    └── DeleteIssueWorkflowTests.cs
```

**Test method pattern:**

```csharp
[Fact]
public async Task CreateIssue_WithValidData_PersistsAndDisplaysInList()
{
    // Arrange
    var page = await context.NewPageAsync();
    await page.GotoAsync("https://localhost:7001/issues");

    // Act
    await page.ClickAsync("button:has-text('New Issue')");
    await page.FillAsync("input[name='title']", "New Issue");
    await page.FillAsync("textarea[name='description']", "Issue description");
    await page.ClickAsync("button:has-text('Create')");
    await page.WaitForURLAsync("**/issues/**");

    // Assert
    await Assertions.Expect(page).ToHaveTitleAsync(new Regex("New Issue"));
    var title = await page.TextContentAsync("h1");
    title.Should().Contain("New Issue");
}
```

---

## Coverage Thresholds

Define measurable coverage expectations per category:

| Category | Metric | Target | Verification |
|----------|--------|--------|--------------|
| **Business Logic** | Command/Query handlers, validators, domain services | 80%+ | Coverage.py, OpenCover report in CI |
| **UI Components** | Blazor component rendering, interaction | 60%+ | bUnit test coverage report |
| **Architecture** | Layering rules, dependency rules, naming conventions | 100% | NetArchTest.Rules (all pass/fail) |
| **E2E Happy Path** | User workflows (create, read, update, delete) | 100% | Playwright test matrix (all pass) |
| **Integration** | Repository layer, full slice workflows | 60%+ | Integration test coverage report |

**Coverage aggregation:**
- Unit + Integration + BlazorTests coverage combined should reach 75%+ overall
- Excluded from coverage: trivial getters/setters, auto-generated code
- Target is sustainable coverage, not 100% (diminishing returns after 80%)

---

## Test Pyramid & Priority

The testing strategy follows the traditional test pyramid for optimal speed and feedback:

```
       ⬜ E2E (Slow, Few)
      🟩🟩 Integration (Moderate, Some)
    🟦🟦🟦🟦 Unit (Fast, Many)
   🟨🟨🟨🟨🟨 Architecture (Instant, All)
```

### Execution Priority (in CI/CD):

1. **Layer 1 — Architecture Tests** (runs first, ~5 seconds)
   - Instant feedback on structural violations
   - Required to pass before proceeding to other tests
   - Run on every commit

2. **Layer 2 — Unit Tests** (parallel with build, ~3 minutes)
   - Fast execution, no external dependencies
   - Run in parallel (sharded by test project)
   - Developers run locally before committing

3. **Layer 3 — Integration Tests** (after build, ~2 minutes)
   - Require MongoDB, slightly slower
   - Run in parallel (one job per test project)
   - Catch integration issues early

4. **Layer 4 — Blazor Component Tests** (after build, ~1 minute)
   - bUnit rendering tests, very fast
   - Require Web project, but no external services
   - Run in parallel

5. **Layer 5 — Aspire Orchestration Tests** (after build, ~1 minute)
   - App host configuration validation
   - Run in parallel

6. **Layer 6 — E2E Tests** (final gate before merge, ~3 minutes)
   - Browser automation, slowest layer
   - Only run on main/squad/* branches or manual trigger
   - Run in parallel (multiple browser instances)

**Total CI/CD time:** ~8–10 minutes (parallel execution)

---

## CQRS Testing Patterns

### Command Handler Tests

Commands represent state-changing operations. Test patterns:

```csharp
[Fact]
public async Task CreateIssue_WithValidCommand_ReturnsNewId()
{
    // Arrange
    var command = new CreateIssueCommand
    {
        Title = "New Issue",
        Description = "Description"
    };
    
    var repository = Substitute.For<IIssueRepository>();
    repository.AddAsync(Arg.Any<Issue>()).Returns(Task.CompletedTask);
    
    var handler = new CreateIssueHandler(repository);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().BeGreaterThan(0);
    await repository.Received(1).AddAsync(Arg.Is<Issue>(i => i.Title == "New Issue"));
}

[Fact]
public async Task CreateIssue_WithEmptyTitle_ReturnsFailed()
{
    // Arrange
    var command = new CreateIssueCommand { Title = "", Description = "Desc" };
    var validator = new CreateIssueValidator();

    // Act
    var result = validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateIssueCommand.Title));
}
```

**Patterns:**
- Test happy path (valid input → success)
- Test validation failures (invalid input → error)
- Test command effects via mocked repository (Received() assertions)
- Mock repository/service dependencies with NSubstitute

### Query Handler Tests

Queries retrieve data without state changes. Test patterns:

```csharp
[Fact]
public async Task GetIssueById_WithExistingId_ReturnsIssue()
{
    // Arrange
    var id = 123;
    var issue = new Issue { Id = id, Title = "Test" };
    
    var repository = Substitute.For<IIssueRepository>();
    repository.GetByIdAsync(id).Returns(issue);
    
    var handler = new GetIssueByIdHandler(repository);
    var query = new GetIssueByIdQuery { Id = id };

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Title.Should().Be("Test");
}

[Fact]
public async Task GetIssueById_WithNonExistentId_ReturnsNotFound()
{
    // Arrange
    var repository = Substitute.For<IIssueRepository>();
    repository.GetByIdAsync(999).Returns((Issue?)null);
    
    var handler = new GetIssueByIdHandler(repository);
    var query = new GetIssueByIdQuery { Id = 999 };

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Error.Should().Be("Issue not found");
}
```

**Patterns:**
- Test data retrieval with mocked repository
- Test filtering/transformation logic
- Test null/not-found scenarios
- Verify correct query parameters passed to repository

### Validator Tests

FluentValidation validators enforce business rules. Test patterns:

```csharp
public class CreateIssueValidatorTests
{
    private readonly IValidator<CreateIssueCommand> _validator = new CreateIssueValidator();

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithMissingTitle_HasError(string? title)
    {
        // Arrange
        var command = new CreateIssueCommand { Title = title, Description = "Desc" };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CreateIssueCommand.Title));
    }

    [Theory]
    [InlineData("x")]
    [InlineData("Valid Title")]
    [InlineData("A very long but valid title that is still under the character limit")]
    public void Validate_WithValidTitle_HasNoError(string title)
    {
        // Arrange
        var command = new CreateIssueCommand { Title = title, Description = "Desc" };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CreateIssueCommand.Title));
    }

    [Fact]
    public void Validate_WithDescriptionTooLong_HasError()
    {
        // Arrange
        var command = new CreateIssueCommand 
        { 
            Title = "Valid",
            Description = new string('x', 5001) // Over 5000 character limit
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CreateIssueCommand.Description));
    }
}
```

**Patterns:**
- Use [Theory] with [InlineData] for boundary testing
- Test each validation rule independently
- Test null, empty, whitespace edge cases
- Test valid and invalid scenarios for each rule
- Verify error messages are meaningful

---

## CI/CD Integration

### Workflow Structure (squad-test.yml)

The test suite runs in the following job matrix:

```yaml
jobs:
  build:              # Compile solution (1 job, ~5 min)
    - Restore & build in Release mode
    
  test-unit:          # Unit tests (parallel, ~3 min)
  test-integration:   # Integration tests (parallel, ~2 min)
  test-architecture:  # Architecture rules (parallel, ~1 min)
  test-blazor:        # Blazor component tests (parallel, ~1 min)
  test-aspire:        # Aspire tests (parallel, ~1 min)
  test-e2e:           # E2E Playwright tests (parallel, ~3 min)
```

All test jobs depend on `build` job, run in parallel after build completes.

### Which Tests Run Where

| Workflow | Trigger | Tests Included | Purpose |
|----------|---------|---|---------|
| **squad-ci.yml** | PR #open, PR #edit | Unit, Architecture | Quick smoke test for PR validation |
| **squad-test.yml** | Push to main, squad/* | All (Unit, Integration, Blazor, Aspire, E2E, Architecture) | Full suite on merge |
| **Local (developer)** | Before commit | Unit (via `dotnet test Unit.csproj`) | Fast feedback loop |

### Parallel Execution Strategy

**Job matrix (squad-test.yml):**

```yaml
test-unit:
  runs-on: ubuntu-latest
  needs: build
  steps:
    - run: dotnet test tests/Unit/Unit.csproj --no-build --configuration Release

test-integration:
  runs-on: ubuntu-latest
  needs: build
  steps:
    - run: dotnet test tests/Integration/Integration.csproj --no-build --configuration Release

test-architecture:
  runs-on: ubuntu-latest
  needs: build
  steps:
    - run: dotnet test tests/Architecture/Architecture.csproj --no-build --configuration Release
```

Each test job runs independently after the build completes, providing parallel execution and reducing total CI time.

### Coverage Reporting

Coverage metrics are collected and reported:

```yaml
test-unit:
  steps:
    - run: dotnet test ... /p:CollectCoverage=true /p:CoverageFormat=opencover
    - uses: codecov/codecov-action@v4
      with:
        files: ./coverage.opencover.xml
        flags: unit
```

**Aggregation:** All coverage reports combined → Single project coverage metric in codecov

### Failure Handling

- **Unit test failure:** Stops PR merge, requires fix
- **Integration test failure:** Stops PR merge, indicates data layer issue
- **Architecture test failure:** Stops build entirely, blocks all PRs
- **E2E test failure:** On main/squad/* only; blocks release, requires investigation

All failures trigger Slack notifications (via squad-heartbeat.yml).

---

## Acceptance Criteria (for the Team)

These are mandatory requirements that the team follows to maintain test quality:

### For All New Features

✅ **Every vertical slice must include:**
- At least one unit test per command/query handler
- At least one integration test validating the full slice
- Validators tested independently with [Theory] tests covering all rules
- 80%+ code coverage for business logic in the slice

✅ **Before PR merge:**
- All tests pass locally (`dotnet test` in test directory)
- Architecture tests pass (NetArchTest.Rules)
- No test skips or TODO tests in the PR
- Code follows naming/structure conventions (see patterns section)
- At least one reviewer confirms test coverage is sufficient

### For Architecture & Layering

✅ **Every project must:**
- Define architecture rules in Architecture.csproj (naming conventions, layering)
- Maintain zero circular dependencies
- Follow naming convention (e.g., handlers end with "Handler", validators end with "Validator")
- Not violate layering rules (Domain doesn't reference Api, etc.)

### For E2E Tests

✅ **Every user workflow must have:**
- At least one happy-path E2E test
- Test exercises full stack (UI → API → Database)
- Test verifies data persists and is visible in UI

### For Coverage & Quality

✅ **Coverage targets:**
- Business logic: 80%+ (no new slices below this)
- UI components: 60%+ (Blazor rendering, basic interactions)
- Architecture: 100% (all rules pass every build)
- E2E: 100% happy path coverage (all workflows tested)

✅ **No pull requests merge without:**
- 80%+ business logic coverage in the changed code
- All architecture tests passing
- All modified tests passing

---

## Examples & Templates

### Example 1: Unit Test Template

**File:** `tests/Unit/Handlers/Commands/CreateIssueHandlerTests.cs`

```csharp
namespace IssueManager.Tests.Unit.Handlers.Commands;

public class CreateIssueHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateIssueCommand
        {
            Title = "New Issue",
            Description = "Test description"
        };

        var mockRepository = Substitute.For<IIssueRepository>();
        mockRepository.AddAsync(Arg.Any<Issue>()).Returns(Task.CompletedTask);

        var handler = new CreateIssueHandler(mockRepository);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_WithInvalidInput_ReturnsFailed()
    {
        // Arrange
        var command = new CreateIssueCommand { Title = "", Description = "Desc" };
        var validator = new CreateIssueValidator();

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
```

### Example 2: Integration Test Template

**File:** `tests/Integration/Handlers/Commands/CreateIssueHandlerTests.cs`

```csharp
namespace IssueManager.Tests.Integration.Handlers.Commands;

[Collection("MongoDB collection")]
public class CreateIssueHandlerTests : IAsyncLifetime
{
    private readonly MongoDbFixture _fixture = new();
    
    public async Task InitializeAsync() => await _fixture.InitializeAsync();
    public async Task DisposeAsync() => await _fixture.DisposeAsync();

    [Fact]
    public async Task Handle_WithValidCommand_PersistsToDatabase()
    {
        // Arrange
        var dbContext = new IssueManagerDbContext(_fixture.Options);
        var command = new CreateIssueCommand { Title = "Integration Test", Description = "..." };
        var handler = new CreateIssueHandler(new IssueRepository(dbContext));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var persisted = await dbContext.Issues.FirstOrDefaultAsync(i => i.Id == result.Value);
        persisted.Should().NotBeNull();
        persisted!.Title.Should().Be("Integration Test");
    }
}
```

### Example 3: Blazor Component Test Template

**File:** `tests/BlazorTests/Components/IssueFormComponentTests.cs`

```csharp
namespace IssueManager.Tests.BlazorTests.Components;

public class IssueFormComponentTests
{
    [Fact]
    public void Render_WithValidParameters_DisplaysForm()
    {
        // Arrange
        using var ctx = new TestContext();

        // Act
        var cut = ctx.RenderComponent<IssueFormComponent>();

        // Assert
        cut.Find("form").Should().NotBeNull();
        cut.Find("input[type='text']").Should().NotBeNull();
        cut.Find("textarea").Should().NotBeNull();
    }

    [Fact]
    public async Task SubmitForm_WithValidData_CallsCallback()
    {
        // Arrange
        using var ctx = new TestContext();
        var submitted = false;

        var cut = ctx.RenderComponent<IssueFormComponent>(
            p => p.Add(c => c.OnSubmit, new EventCallback<CreateIssueCommand>(
                null, new Func<CreateIssueCommand, Task>(async c => {
                    submitted = true;
                    await Task.CompletedTask;
                })
            ))
        );

        // Act
        var form = cut.Find("form");
        await form.SubmitAsync();

        // Assert
        submitted.Should().BeTrue();
    }
}
```

### Example 4: E2E Test Template

**File:** `tests/E2E/Workflows/CreateIssueWorkflowTests.cs`

```csharp
namespace IssueManager.Tests.E2E.Workflows;

[Collection("Playwright")]
public class CreateIssueWorkflowTests
{
    private readonly BrowserFixture _fixture;

    public CreateIssueWorkflowTests(BrowserFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task CreateIssue_WithValidData_DisplaysInList()
    {
        // Arrange
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync("https://localhost:7001/issues");

        // Act
        await page.ClickAsync("button:has-text('New Issue')");
        await page.FillAsync("input[name='title']", "E2E Test Issue");
        await page.FillAsync("textarea[name='description']", "Test description");
        await page.ClickAsync("button[type='submit']");
        await page.WaitForURLAsync("**/issues/**");

        // Assert
        var title = await page.TextContentAsync("h1");
        title.Should().Contain("E2E Test Issue");
        
        await page.GotoAsync("https://localhost:7001/issues");
        var listContent = await page.ContentAsync();
        listContent.Should().Contain("E2E Test Issue");

        await page.CloseAsync();
    }
}
```

---

## Implementation Timeline

### Phase 1: Coverage Baseline (Weeks 1–2)

- [ ] Run all tests with coverage reporting enabled
- [ ] Generate coverage reports for each test category
- [ ] Document current baseline coverage per slice
- [ ] Identify gaps (slices with <80% business logic coverage)
- [ ] Create tracking table of coverage targets by slice

### Phase 2: CI/CD Integration (Weeks 2–4)

- [ ] Add coverage reporting to squad-test.yml (codecov integration)
- [ ] Fix E2E test environment setup (Playwright browser install in CI)
- [ ] Configure parallel job execution for all test categories
- [ ] Set up coverage aggregation across test projects
- [ ] Add badges to README (coverage %, test count, build status)
- [ ] Configure failure notifications for coverage drops

### Phase 3: Patterns & Templates (Weeks 3–5)

- [ ] Document test naming conventions across all projects
- [ ] Create reusable fixtures (MongoDbFixture, BrowserFixture) as templates
- [ ] Add test templates to each test project (TestTemplates/ directory)
- [ ] Update CONTRIBUTING.md with test section
- [ ] Create code review checklist for test coverage validation

### Phase 4: Enforcement & Training (Weeks 5–6)

- [ ] Add coverage checks to PR requirements (GitHub branch protection)
- [ ] Configure architecture test enforcement on main branch
- [ ] Document patterns in team wiki/onboarding guide
- [ ] Run team training on test-driven development (TDD)
- [ ] Update PR template to include test checklist

### Phase 5: Ongoing (Post-implementation)

- [ ] Monitor coverage trends in CI/CD
- [ ] Review test failures in team standups
- [ ] Refactor test utilities based on team feedback
- [ ] Expand E2E test suite as new features launch

---

## Summary

This PRD establishes a comprehensive, sustainable testing strategy for IssueManager that:

1. **Clarifies test responsibilities** — Each test type has a clear purpose, scope, and coverage target
2. **Provides concrete patterns** — Developers have templates and examples for writing tests
3. **Integrates with CI/CD** — Tests run in parallel, providing fast feedback (8–10 minutes total)
4. **Enforces quality standards** — Acceptance criteria ensure all new features meet coverage thresholds
5. **Supports CQRS architecture** — Specific patterns for commands, queries, handlers, and validators
6. **Scales with the project** — Test infrastructure grows as features are added

**Next Steps for the Team:**
1. Review and approve this PRD in team standup
2. Begin Phase 1: Coverage baseline assessment
3. Assign owner for each phase (recommended: Gimli for Phase 2, Aragorn for Phase 3)
4. Track progress in GitHub Issues with phase-specific tasks

---

**Document Version:** 1.0.0  
**Last Updated:** 2026-02-19  
**Approved By:** (Pending team review)
