# Integration Testing Guide

## Overview

Integration tests verify that multiple components work together correctly. In IssueManager, this means testing full vertical slices: validator → handler → repository → MongoDB.

**When to use integration tests:**
- Testing handlers with real MongoDB persistence
- Testing full CQRS vertical slices (command/query execution)
- Testing repository methods with real database operations
- Testing data serialization and deserialization
- Verifying database constraints and indexes

**Frameworks used:**
- **xUnit** — Test runner
- **TestContainers** — Ephemeral MongoDB containers
- **FluentAssertions** — Readable assertions
- **MongoDB.Driver** — Database client

## Setup

### TestContainers
TestContainers spins up a real MongoDB instance in a Docker container for each test run. This gives us:
- Real database behavior (no mocking)
- Isolated test data (container is destroyed after tests)
- Fast setup (container is reused within a test class)

### Create an Integration Test File

1. Add test file to `tests/Integration/Handlers/`
2. Implement `IAsyncLifetime` for container lifecycle
3. Reference frameworks via GlobalUsings:
   ```csharp
   // tests/Integration/GlobalUsings.cs
   global using Xunit;
   global using FluentAssertions;
   global using Testcontainers.MongoDb;
   global using IssueManager.Shared.Domain;
   global using IssueManager.Shared.Handlers;
   global using IssueManager.Shared.Validators;
   global using IssueManager.Persistence.MongoDb;
   ```

## Example: Testing CreateIssueHandler

**Real example from the codebase:** [`tests/Integration/Handlers/CreateIssueHandlerTests.cs`](../../tests/Integration/Handlers/CreateIssueHandlerTests.cs)

```csharp
namespace IssueManager.Tests.Integration.Handlers;

/// <summary>
/// Integration tests for CreateIssueHandler with real MongoDB database.
/// </summary>
public class CreateIssueHandlerTests : IAsyncLifetime
{
    private const string MONGODB_IMAGE = "mongo:8.0";
    private const string TEST_DATABASE = "IssueManagerTestDb";
    private readonly MongoDbContainer _mongoContainer;

    private IIssueRepository _repository = null!;
    private CreateIssueHandler _handler = null!;

    public CreateIssueHandlerTests()
    {
        _mongoContainer = new MongoDbBuilder()
            .WithImage(MONGODB_IMAGE)
            .Build();
    }

    /// <summary>
    /// Initializes the test container and repository.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();
        var connectionString = _mongoContainer.GetConnectionString();
        _repository = new IssueRepository(connectionString, TEST_DATABASE);
        _handler = new CreateIssueHandler(_repository, new CreateIssueValidator());
    }

    /// <summary>
    /// Disposes the test container.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _mongoContainer.StopAsync();
        await _mongoContainer.DisposeAsync();
    }

    [Fact]
    public async Task Handle_ValidCommand_StoresIssueInDatabase()
    {
        // Arrange
        var command = new CreateIssueCommand
        {
            Title = "Test Issue",
            Description = "This is a test issue description."
        };

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Title.Should().Be("Test Issue");
        result.Description.Should().Be("This is a test issue description.");
        result.Status.Should().Be(IssueStatus.Open);

        // Verify persistence
        var retrieved = await _repository.GetByIdAsync(result.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Title.Should().Be("Test Issue");
    }
}
```

## Test Lifecycle with IAsyncLifetime

### Container Setup (InitializeAsync)
```csharp
public async Task InitializeAsync()
{
    // 1. Start MongoDB container
    await _mongoContainer.StartAsync();

    // 2. Get connection string
    var connectionString = _mongoContainer.GetConnectionString();

    // 3. Initialize repository
    _repository = new IssueRepository(connectionString, TEST_DATABASE);

    // 4. Initialize handler with real dependencies
    _handler = new CreateIssueHandler(_repository, new CreateIssueValidator());
}
```

### Container Teardown (DisposeAsync)
```csharp
public async Task DisposeAsync()
{
    // Stop and dispose container
    await _mongoContainer.StopAsync();
    await _mongoContainer.DisposeAsync();
}
```

## Testing Full Vertical Slices

### Test Pattern: Given-When-Then
```csharp
[Fact]
public async Task Handle_ValidCommand_StoresIssueInDatabase()
{
    // Given — Test data and preconditions
    var command = new CreateIssueCommand
    {
        Title = "Test Issue",
        Description = "This is a test issue description."
    };

    // When — Execute the handler
    var result = await _handler.Handle(command);

    // Then — Verify outcome
    result.Should().NotBeNull();
    result.Id.Should().NotBeEmpty();
    result.Title.Should().Be("Test Issue");

    // And — Verify persistence
    var retrieved = await _repository.GetByIdAsync(result.Id);
    retrieved.Should().NotBeNull();
    retrieved!.Title.Should().Be("Test Issue");
}
```

### Verify Persistence
Always verify that data was actually saved:
```csharp
// Act
var result = await _handler.Handle(command);

// Assert — Check return value
result.Should().NotBeNull();

// Assert — Verify database persistence
var retrieved = await _repository.GetByIdAsync(result.Id);
retrieved.Should().NotBeNull();
retrieved!.Title.Should().Be(command.Title);
```

## Testing Validation Integration

### Test That Validation Errors Are Thrown
```csharp
[Fact]
public async Task Handle_EmptyTitle_ThrowsValidationException()
{
    // Arrange
    var command = new CreateIssueCommand
    {
        Title = "",
        Description = "Description without title"
    };

    // Act & Assert
    await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command));
}

[Fact]
public async Task Handle_TitleTooShort_ThrowsValidationException()
{
    // Arrange
    var command = new CreateIssueCommand
    {
        Title = "AB", // Min is 3
        Description = "Title is too short"
    };

    // Act & Assert
    await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command));
}
```

## Testing Data Persistence

### Test Multiple Entities
```csharp
[Fact]
public async Task Handle_MultipleIssues_AllPersistedCorrectly()
{
    // Arrange & Act
    var issue1 = await _handler.Handle(new CreateIssueCommand { Title = "First Issue" });
    var issue2 = await _handler.Handle(new CreateIssueCommand { Title = "Second Issue" });
    var issue3 = await _handler.Handle(new CreateIssueCommand { Title = "Third Issue" });

    // Assert
    var count = await _repository.CountAsync();
    count.Should().Be(3);

    var allIssues = await _repository.GetAllAsync();
    allIssues.Should().HaveCount(3);
    allIssues.Should().Contain(i => i.Title == "First Issue");
    allIssues.Should().Contain(i => i.Title == "Second Issue");
    allIssues.Should().Contain(i => i.Title == "Third Issue");
}
```

### Test Complex Objects (Labels, etc.)
```csharp
[Fact]
public async Task Handle_ValidCommandWithLabels_StoresIssueWithLabels()
{
    // Arrange
    var command = new CreateIssueCommand
    {
        Title = "Bug Report",
        Description = "Found a critical bug",
        Labels = new List<string> { "bug", "critical", "backend" }
    };

    // Act
    var result = await _handler.Handle(command);

    // Assert
    result.Labels.Should().HaveCount(3);
    result.Labels.Should().Contain(l => l.Name == "bug");
    result.Labels.Should().Contain(l => l.Name == "critical");
    result.Labels.Should().Contain(l => l.Name == "backend");

    // Verify persistence
    var retrieved = await _repository.GetByIdAsync(result.Id);
    retrieved!.Labels.Should().HaveCount(3);
}
```

### Test Timestamps and Metadata
```csharp
[Fact]
public async Task Handle_CreatedIssue_HasCorrectTimestamps()
{
    // Arrange
    var beforeCreation = DateTime.UtcNow.AddSeconds(-1);
    var command = new CreateIssueCommand
    {
        Title = "Timestamp Test Issue"
    };

    // Act
    var result = await _handler.Handle(command);
    var afterCreation = DateTime.UtcNow.AddSeconds(1);

    // Assert
    result.CreatedAt.Should().BeAfter(beforeCreation);
    result.CreatedAt.Should().BeBefore(afterCreation);
    result.UpdatedAt.Should().BeAfter(beforeCreation);
    result.UpdatedAt.Should().BeBefore(afterCreation);
    result.CreatedAt.Should().BeCloseTo(result.UpdatedAt, TimeSpan.FromSeconds(1));
}
```

## Test Data Management

### Isolation Between Tests
Each test should create its own data:
```csharp
// Good — Each test creates its own issue
[Fact]
public async Task Test1()
{
    var issue = await _handler.Handle(new CreateIssueCommand { Title = "Test1 Issue" });
    // Test issue
}

[Fact]
public async Task Test2()
{
    var issue = await _handler.Handle(new CreateIssueCommand { Title = "Test2 Issue" });
    // Test issue
}
```

### Unique IDs
Use unique identifiers to avoid collisions:
```csharp
var command = new CreateIssueCommand
{
    Title = $"Test Issue {Guid.NewGuid()}"
};
```

### Cleanup
TestContainers automatically destroys the container after tests, so no manual cleanup is needed. However, if you need to clean up within a test:
```csharp
// Delete test data
await _repository.DeleteAsync(issueId);
```

## Shared MongoDB Fixture (Advanced)

For faster test runs, share a container across multiple test classes:

**`tests/Integration/Fixtures/MongoDbFixture.cs`:**
```csharp
public class MongoDbFixture : IAsyncLifetime
{
    private const string MONGODB_IMAGE = "mongo:8.0";
    private readonly MongoDbContainer _mongoContainer;

    public MongoDbFixture()
    {
        _mongoContainer = new MongoDbBuilder()
            .WithImage(MONGODB_IMAGE)
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

**Use with xUnit Collection Fixture:**
```csharp
[CollectionDefinition("MongoDB")]
public class MongoDbCollection : ICollectionFixture<MongoDbFixture>
{
}

[Collection("MongoDB")]
public class CreateIssueHandlerTests
{
    private readonly MongoDbFixture _mongoFixture;

    public CreateIssueHandlerTests(MongoDbFixture mongoFixture)
    {
        _mongoFixture = mongoFixture;
    }

    // Tests use _mongoFixture.ConnectionString
}
```

## Performance Tuning

### Container Startup Time
- **First run:** ~5-10 seconds (Docker pull + container start)
- **Subsequent runs:** ~2-3 seconds (cached image)
- **Shared fixture:** Amortizes startup across tests

### Parallel Execution
xUnit runs test classes in parallel by default. Each class gets its own container.

### Optimize Container Configuration
```csharp
_mongoContainer = new MongoDbBuilder()
    .WithImage(MONGODB_IMAGE)
    .WithCleanUp(true) // Auto-cleanup
    .Build();
```

## Best Practices

### ✅ Do
- **Use real database** — TestContainers gives you MongoDB behavior
- **Test full vertical slices** — Validator → Handler → Repository
- **Verify persistence** — Always check data was saved
- **Isolate test data** — Each test creates its own entities
- **Test validation integration** — Ensure validators are called
- **Use descriptive test names** — `Handle_ValidCommand_StoresIssueInDatabase`

### ❌ Don't
- **Mock the database** — Use TestContainers for real integration tests
- **Share state between tests** — Each test should be independent
- **Skip cleanup** — TestContainers handles this, but be aware
- **Test only happy paths** — Also test validation failures

## Common Mistakes

### ❌ Not Verifying Persistence
```csharp
// Bad — Only checks return value
var result = await _handler.Handle(command);
result.Should().NotBeNull();
```

### ✅ Always Verify Persistence
```csharp
// Good — Verifies database persistence
var result = await _handler.Handle(command);
result.Should().NotBeNull();

var retrieved = await _repository.GetByIdAsync(result.Id);
retrieved.Should().NotBeNull();
```

### ❌ Shared Mutable State
```csharp
// Bad — Shared state across tests
private Issue _sharedIssue = new Issue { /* ... */ };

[Fact]
public async Task Test1() { /* Modifies _sharedIssue */ }

[Fact]
public async Task Test2() { /* Also modifies _sharedIssue */ }
```

### ✅ Independent Test Data
```csharp
// Good — Each test creates its own data
[Fact]
public async Task Test1()
{
    var issue = await CreateTestIssue("Test1");
    // Test issue
}

[Fact]
public async Task Test2()
{
    var issue = await CreateTestIssue("Test2");
    // Test issue
}
```

## Debugging Integration Test Failures

### Check MongoDB Container Logs
```bash
docker logs <container_id>
```

### Verify Connection String
```csharp
Console.WriteLine($"Connection string: {_mongoContainer.GetConnectionString()}");
```

### Add Debug Logging to Handler/Repository
```csharp
// In handler
Console.WriteLine($"Creating issue: {command.Title}");
var result = await _repository.CreateAsync(issue);
Console.WriteLine($"Created issue with ID: {result.Id}");
```

### Use Test Explorer in Visual Studio
- Set breakpoints in tests
- Run tests in Debug mode
- Inspect variables and step through code

## Running Integration Tests

```bash
# Run all integration tests
dotnet test tests/Integration

# Run specific test
dotnet test --filter "FullyQualifiedName~CreateIssueHandlerTests"

# Run with verbose output
dotnet test tests/Integration --logger "console;verbosity=detailed"

# Run in watch mode
dotnet watch test --project tests/Integration
```

### Running Locally vs. CI

**Local:**
- Docker must be running
- Container images are cached after first run
- Fast feedback loop

**CI (GitHub Actions, etc.):**
- Docker-in-Docker or Docker socket mount
- Container images are cached per build
- Parallel test execution

## See Also

- [Testing Strategy](../TESTING.md) — Overall test philosophy
- [Unit Testing Guide](UNIT-TESTS.md) — Testing validators in isolation
- [Test Data & Fixtures Guide](TEST-DATA.md) — Data management patterns
- [TestContainers .NET Documentation](https://testcontainers.com/)

---

**Real examples in the codebase:**
- [`tests/Integration/Handlers/CreateIssueHandlerTests.cs`](../../tests/Integration/Handlers/CreateIssueHandlerTests.cs)
- [`tests/Integration/Handlers/GetIssueHandlerTests.cs`](../../tests/Integration/Handlers/GetIssueHandlerTests.cs)
- [`tests/Integration/Handlers/UpdateIssueStatusHandlerTests.cs`](../../tests/Integration/Handlers/UpdateIssueStatusHandlerTests.cs)
- [`tests/Integration/Fixtures/MongoDbFixture.cs`](../../tests/Integration/Fixtures/MongoDbFixture.cs)
