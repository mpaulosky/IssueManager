# Test Data and Fixtures Guide

## Overview

Test data management is critical for reliable, isolated tests. This guide covers patterns for creating test data, managing fixtures, and ensuring test isolation.

**Key principles:**
- **Isolation** — Each test creates its own data
- **Repeatability** — Tests produce the same results every run
- **Clarity** — Test data is easy to understand
- **Performance** — Setup is fast, cleanup is automatic

## Test Data Patterns

### Inline Test Data
Simplest approach: create data directly in the test.

```csharp
[Fact]
public async Task CreateIssue_ValidData_Succeeds()
{
    // Arrange — Inline test data
    var command = new CreateIssueCommand
    {
        Title = "Test Issue",
        Description = "Test description",
        Labels = new List<string> { "bug", "urgent" }
    };

    // Act
    var result = await _handler.Handle(command);

    // Assert
    result.Should().NotBeNull();
    result.Title.Should().Be("Test Issue");
}
```

**When to use:**
- Simple data
- Data is specific to one test
- Clarity is more important than DRY

### Test Data Builders
Builder pattern for complex objects.

```csharp
public class CreateIssueCommandBuilder
{
    private string _title = "Default Title";
    private string? _description = null;
    private List<string> _labels = new();

    public CreateIssueCommandBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public CreateIssueCommandBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public CreateIssueCommandBuilder WithLabels(params string[] labels)
    {
        _labels = labels.ToList();
        return this;
    }

    public CreateIssueCommand Build()
    {
        return new CreateIssueCommand
        {
            Title = _title,
            Description = _description,
            Labels = _labels
        };
    }
}

// Usage
[Fact]
public async Task CreateIssue_WithLabels_Succeeds()
{
    // Arrange
    var command = new CreateIssueCommandBuilder()
        .WithTitle("Bug Report")
        .WithDescription("Critical issue")
        .WithLabels("bug", "critical")
        .Build();

    // Act
    var result = await _handler.Handle(command);

    // Assert
    result.Labels.Should().HaveCount(2);
}
```

**When to use:**
- Complex objects with many properties
- Multiple tests need similar data with variations
- Readable, fluent API is valuable

### Factory Methods
Static methods or helpers to create common test data.

```csharp
public static class TestDataFactory
{
    public static CreateIssueCommand CreateValidIssueCommand(string? title = null)
    {
        return new CreateIssueCommand
        {
            Title = title ?? $"Test Issue {Guid.NewGuid()}",
            Description = "Test description"
        };
    }

    public static CreateIssueCommand CreateIssueCommandWithLabels(params string[] labels)
    {
        return new CreateIssueCommand
        {
            Title = $"Issue with Labels {Guid.NewGuid()}",
            Description = "Test description",
            Labels = labels.ToList()
        };
    }

    public static Issue CreateIssue(string? title = null, IssueStatus status = IssueStatus.Open)
    {
        return new Issue
        {
            Id = Guid.NewGuid().ToString(),
            Title = title ?? "Test Issue",
            Description = "Test description",
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Labels = new List<Label>()
        };
    }
}

// Usage
[Fact]
public async Task GetIssue_ExistingId_ReturnsIssue()
{
    // Arrange
    var issue = TestDataFactory.CreateIssue("My Issue");
    await _repository.AddAsync(issue);

    // Act
    var result = await _repository.GetByIdAsync(issue.Id);

    // Assert
    result.Should().NotBeNull();
    result!.Title.Should().Be("My Issue");
}
```

**When to use:**
- Common data patterns reused across many tests
- Simple, readable API
- Don't need fluent chaining

### Object Mother Pattern
Provides pre-configured objects for common scenarios.

```csharp
public static class IssueMotherObject
{
    public static Issue OpenIssue() => new Issue
    {
        Id = Guid.NewGuid().ToString(),
        Title = "Open Issue",
        Status = IssueStatus.Open,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        Labels = new List<Label>()
    };

    public static Issue ClosedIssue() => new Issue
    {
        Id = Guid.NewGuid().ToString(),
        Title = "Closed Issue",
        Status = IssueStatus.Closed,
        CreatedAt = DateTime.UtcNow.AddDays(-7),
        UpdatedAt = DateTime.UtcNow,
        Labels = new List<Label>()
    };

    public static Issue IssueWithLabels(params string[] labels) => new Issue
    {
        Id = Guid.NewGuid().ToString(),
        Title = "Issue with Labels",
        Status = IssueStatus.Open,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        Labels = labels.Select(l => new Label(l)).ToList()
    };
}

// Usage
[Fact]
public async Task UpdateStatus_ClosedIssue_Succeeds()
{
    // Arrange
    var issue = IssueMotherObject.ClosedIssue();
    await _repository.AddAsync(issue);

    // Act
    var command = new UpdateIssueStatusCommand
    {
        Id = issue.Id,
        Status = IssueStatus.Open
    };
    var result = await _handler.Handle(command);

    // Assert
    result.Status.Should().Be(IssueStatus.Open);
}
```

**When to use:**
- Common domain scenarios (open issue, closed issue, etc.)
- Named methods document intent
- Multiple tests need the same "flavor" of object

## Fixtures

### xUnit IAsyncLifetime
For setup/teardown that runs once per test class.

```csharp
public class CreateIssueHandlerTests : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer;
    private IIssueRepository _repository = null!;
    private CreateIssueHandler _handler = null!;

    public CreateIssueHandlerTests()
    {
        _mongoContainer = new MongoDbBuilder()
            .WithImage("mongo:8.0")
            .Build();
    }

    public async Task InitializeAsync()
    {
        // Setup — Runs before each test
        await _mongoContainer.StartAsync();
        var connectionString = _mongoContainer.GetConnectionString();
        _repository = new IssueRepository(connectionString, "TestDb");
        _handler = new CreateIssueHandler(_repository, new CreateIssueValidator());
    }

    public async Task DisposeAsync()
    {
        // Teardown — Runs after each test
        await _mongoContainer.StopAsync();
        await _mongoContainer.DisposeAsync();
    }
}
```

### xUnit Collection Fixtures
Share fixtures across multiple test classes.

**Define the fixture:**
```csharp
public class MongoDbFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer;

    public MongoDbFixture()
    {
        _mongoContainer = new MongoDbBuilder()
            .WithImage("mongo:8.0")
            .Build();
    }

    public string ConnectionString => _mongoContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _mongoContainer.StopAsync();
        await _mongoContainer.DisposeAsync();
    }
}
```

**Define the collection:**
```csharp
[CollectionDefinition("MongoDB")]
public class MongoDbCollection : ICollectionFixture<MongoDbFixture>
{
}
```

**Use in test classes:**
```csharp
[Collection("MongoDB")]
public class CreateIssueHandlerTests
{
    private readonly MongoDbFixture _mongoFixture;
    private IIssueRepository _repository = null!;

    public CreateIssueHandlerTests(MongoDbFixture mongoFixture)
    {
        _mongoFixture = mongoFixture;
        _repository = new IssueRepository(_mongoFixture.ConnectionString, "TestDb");
    }

    [Fact]
    public async Task CreateIssue_ValidData_Succeeds()
    {
        // Test uses _repository
    }
}
```

**When to use:**
- Expensive setup (MongoDB container, API server)
- Multiple test classes need the same resource
- Trade-off: faster tests, but test classes share state

### bUnit ComponentTestBase
Base class for Blazor component tests.

**Real example:** [`tests/BlazorTests/Fixtures/ComponentTestBase.cs`](../../tests/BlazorTests/Fixtures/ComponentTestBase.cs)

```csharp
public abstract class ComponentTestBase : IDisposable
{
    protected TestContext TestContext { get; }

    protected ComponentTestBase()
    {
        TestContext = new TestContext();
        
        // Add common services
        // TestContext.Services.AddSingleton<IMyService>(Substitute.For<IMyService>());
    }

    public void Dispose()
    {
        TestContext?.Dispose();
        GC.SuppressFinalize(this);
    }
}

// Usage
public class IssueFormTests : ComponentTestBase
{
    [Fact]
    public void IssueForm_RendersCorrectly()
    {
        var component = TestContext.RenderComponent<IssueForm>();
        component.Should().NotBeNull();
    }
}
```

## Test Isolation

### Unique Identifiers
Use unique IDs to avoid collisions:

```csharp
[Fact]
public async Task CreateIssue_UniqueTitle_Succeeds()
{
    // Good — Unique ID ensures no conflicts
    var command = new CreateIssueCommand
    {
        Title = $"Test Issue {Guid.NewGuid()}"
    };

    var result = await _handler.Handle(command);
    result.Should().NotBeNull();
}
```

### Per-Test Cleanup
Clean up test data after each test:

```csharp
[Fact]
public async Task CreateAndDeleteIssue_Succeeds()
{
    // Arrange
    var command = new CreateIssueCommand { Title = "Temp Issue" };
    var result = await _handler.Handle(command);

    // Act
    await _repository.DeleteAsync(result.Id);

    // Assert
    var retrieved = await _repository.GetByIdAsync(result.Id);
    retrieved.Should().BeNull();
}
```

### Unique Test Databases
Each test class uses a unique database name:

```csharp
public class CreateIssueHandlerTests : IAsyncLifetime
{
    private const string TEST_DATABASE = "IssueManagerTestDb_CreateIssue";
    // ...
}

public class GetIssueHandlerTests : IAsyncLifetime
{
    private const string TEST_DATABASE = "IssueManagerTestDb_GetIssue";
    // ...
}
```

## MongoDB Test Fixtures

### Ephemeral Containers
TestContainers automatically destroys containers after tests:

```csharp
public async Task DisposeAsync()
{
    await _mongoContainer.StopAsync();
    await _mongoContainer.DisposeAsync(); // Container is deleted
}
```

### Seeding Test Data
Seed data before tests:

```csharp
public async Task InitializeAsync()
{
    await _mongoContainer.StartAsync();
    var connectionString = _mongoContainer.GetConnectionString();
    _repository = new IssueRepository(connectionString, "TestDb");

    // Seed test data
    await _repository.AddAsync(IssueMotherObject.OpenIssue());
    await _repository.AddAsync(IssueMotherObject.ClosedIssue());
}
```

### Collection-Level Cleanup
Drop test collections after tests:

```csharp
public async Task DisposeAsync()
{
    // Clean up test collections
    var client = new MongoClient(_mongoContainer.GetConnectionString());
    var database = client.GetDatabase("TestDb");
    await database.DropCollectionAsync("issues");

    await _mongoContainer.StopAsync();
    await _mongoContainer.DisposeAsync();
}
```

## Mock Services

### NSubstitute Basics
Create mock services for unit tests:

```csharp
[Fact]
public async Task MyService_CallsRepository_ReturnsResult()
{
    // Arrange
    var mockRepo = Substitute.For<IIssueRepository>();
    mockRepo.GetByIdAsync("123").Returns(Task.FromResult<Issue?>(
        new Issue { Id = "123", Title = "Test Issue" }
    ));

    var service = new IssueService(mockRepo);

    // Act
    var result = await service.GetIssueAsync("123");

    // Assert
    result.Should().NotBeNull();
    result!.Title.Should().Be("Test Issue");

    // Verify method was called
    await mockRepo.Received(1).GetByIdAsync("123");
}
```

### Mocking in bUnit Tests
Mock services for Blazor components:

```csharp
public class IssueListTests : ComponentTestBase
{
    [Fact]
    public async Task IssueList_LoadsIssues_RendersCorrectly()
    {
        // Arrange
        var mockService = Substitute.For<IIssueService>();
        mockService.GetAllIssuesAsync().Returns(Task.FromResult<IEnumerable<Issue>>(
            new List<Issue>
            {
                new Issue { Id = "1", Title = "Issue 1" },
                new Issue { Id = "2", Title = "Issue 2" }
            }
        ));

        TestContext.Services.AddSingleton(mockService);

        // Act
        var component = TestContext.RenderComponent<IssueList>();

        // Assert
        component.Markup.Should().Contain("Issue 1");
        component.Markup.Should().Contain("Issue 2");
    }
}
```

## Best Practices

### ✅ Do
- **Create unique test data** — Use GUIDs or timestamps
- **Use builders for complex objects** — Readable, maintainable
- **Clean up resources** — IAsyncLifetime, IDisposable
- **Isolate test data** — Each test creates its own
- **Use factories for common patterns** — DRY principle
- **Document fixture behavior** — XML comments on setup/teardown

### ❌ Don't
- **Share mutable state between tests** — Causes flaky tests
- **Use hardcoded IDs** — Collisions and failures
- **Leave test data in database** — Clean up after tests
- **Over-engineer builders** — Keep it simple
- **Mock everything** — Use real dependencies when possible

## Common Mistakes

### ❌ Shared Mutable State
```csharp
// Bad — Shared state across tests
public class MyTests
{
    private static Issue _sharedIssue = new Issue { /* ... */ };

    [Fact]
    public async Task Test1() { /* Modifies _sharedIssue */ }

    [Fact]
    public async Task Test2() { /* Also modifies _sharedIssue */ }
}
```

### ✅ Independent Test Data
```csharp
// Good — Each test creates its own
public class MyTests
{
    [Fact]
    public async Task Test1()
    {
        var issue = TestDataFactory.CreateIssue();
        // Test issue
    }

    [Fact]
    public async Task Test2()
    {
        var issue = TestDataFactory.CreateIssue();
        // Test issue
    }
}
```

### ❌ Hardcoded IDs
```csharp
// Bad — Collisions if tests run in parallel
var issue = new Issue { Id = "test-123", Title = "Test" };
```

### ✅ Unique IDs
```csharp
// Good — Unique ID
var issue = new Issue { Id = Guid.NewGuid().ToString(), Title = "Test" };
```

## Performance Considerations

### Container Reuse
Share expensive fixtures across test classes:
- Use xUnit Collection Fixtures
- Trade-off: faster tests, but shared state

### Parallel Execution
xUnit runs test classes in parallel:
- Each class gets its own fixture
- Ensure test data is isolated

### Async Initialization
Use `IAsyncLifetime` for async setup:
```csharp
public async Task InitializeAsync()
{
    await _mongoContainer.StartAsync(); // Async
    // ...
}
```

## See Also

- [Testing Strategy](../TESTING.md) — Overall test philosophy
- [Unit Testing Guide](UNIT-TESTS.md) — Testing in isolation
- [Integration Testing Guide](INTEGRATION-TESTS.md) — MongoDB fixtures
- [Blazor Component Testing Guide](BUNIT-BLAZOR-TESTS.md) — Component fixtures

---

**Real examples in the codebase:**
- [`tests/Integration/Fixtures/MongoDbFixture.cs`](../../tests/Integration/Fixtures/MongoDbFixture.cs)
- [`tests/BlazorTests/Fixtures/ComponentTestBase.cs`](../../tests/BlazorTests/Fixtures/ComponentTestBase.cs)
