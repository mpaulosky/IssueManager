using FluentAssertions;
using Api.Data;
using Api.Handlers;
using MongoDB.Bson;
using Shared.DTOs;
using IssueManager.Shared.Validators;
using Testcontainers.MongoDb;

namespace IssueManager.Tests.Integration.Handlers;

/// <summary>
/// Integration tests for ListIssuesHandler with pagination and real MongoDB database.
/// </summary>
public class ListIssuesHandlerIntegrationTests : IAsyncLifetime
{
private const string MONGODB_IMAGE = "mongo:8.0";
private const string TEST_DATABASE = "IssueManagerTestDb";
private readonly MongoDbContainer _mongoContainer;

private IIssueRepository _repository = null!;
private ListIssuesHandler _handler = null!;

public ListIssuesHandlerIntegrationTests()
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
_handler = new ListIssuesHandler(_repository, new ListIssuesQueryValidator());
}

/// <summary>
/// Disposes the test container.
/// </summary>
public async Task DisposeAsync()
{
await _mongoContainer.StopAsync();
await _mongoContainer.DisposeAsync();
}

private static IssueDto CreateTestIssueDto(string title, string description, DateTime? dateCreated = null) =>
new(ObjectId.GenerateNewId(), title, description, dateCreated ?? DateTime.UtcNow, UserDto.Empty, CategoryDto.Empty, StatusDto.Empty);

[Fact]
public async Task Handle_WithPagination_ReturnsCorrectPage()
{
// Arrange - Create 50 issues
for (int i = 0; i < 50; i++)
{
	var issue = CreateTestIssueDto($"Issue {i + 1}", $"Description {i + 1}", DateTime.UtcNow.AddMinutes(-i));
	await _repository.CreateAsync(issue);
}

var query = new ListIssuesQuery { Page = 1, PageSize = 20 };

// Act
var result = await _handler.Handle(query, CancellationToken.None);

// Assert
result.Items.Should().HaveCount(20);
result.Page.Should().Be(1);
result.PageSize.Should().Be(20);
result.Total.Should().Be(50);
result.TotalPages.Should().Be(3); // 50 / 20 = 2.5 → 3 pages
}

[Fact]
public async Task Handle_SecondPage_ReturnsNextSetOfItems()
{
// Arrange - Create 50 issues
for (int i = 0; i < 50; i++)
{
	var issue = CreateTestIssueDto($"Issue {i + 1}", $"Description {i + 1}", DateTime.UtcNow.AddMinutes(-i));
	await _repository.CreateAsync(issue);
}

var query = new ListIssuesQuery { Page = 2, PageSize = 20 };

// Act
var result = await _handler.Handle(query, CancellationToken.None);

// Assert
result.Items.Should().HaveCount(20);
result.Page.Should().Be(2);
result.Total.Should().Be(50);
}

[Fact]
public async Task Handle_ExcludesArchivedIssues()
{
// Arrange - Create 10 issues, archive 3
var issuesToArchive = new List<string>();
for (int i = 0; i < 10; i++)
{
	var issue = CreateTestIssueDto($"Issue {i + 1}", $"Description {i + 1}", DateTime.UtcNow.AddMinutes(-i));
	var created = await _repository.CreateAsync(issue);
	if (i < 3)
		issuesToArchive.Add(created.Id.ToString());
}

foreach (var id in issuesToArchive)
{
	await _repository.ArchiveAsync(id);
}

var query = new ListIssuesQuery { Page = 1, PageSize = 20 };

// Act
var result = await _handler.Handle(query, CancellationToken.None);

// Assert
result.Total.Should().Be(7); // 10 - 3 archived = 7
result.Items.Should().HaveCount(7);
}

[Fact]
public async Task Handle_EmptyDatabase_ReturnsEmptyList()
{
// Arrange - No issues in database
var query = new ListIssuesQuery { Page = 1, PageSize = 20 };

// Act
var result = await _handler.Handle(query, CancellationToken.None);

// Assert
result.Items.Should().BeEmpty();
result.Total.Should().Be(0);
result.TotalPages.Should().Be(0);
}

[Fact]
public async Task Handle_LastPagePartial_ReturnsRemainingItems()
{
// Arrange - Create 42 issues
for (int i = 0; i < 42; i++)
{
	var issue = CreateTestIssueDto($"Issue {i + 1}", $"Description {i + 1}", DateTime.UtcNow.AddMinutes(-i));
	await _repository.CreateAsync(issue);
}

var query = new ListIssuesQuery { Page = 3, PageSize = 20 };

// Act
var result = await _handler.Handle(query, CancellationToken.None);

// Assert
result.Items.Should().HaveCount(2); // 42 - 40 = 2 on last page
result.Page.Should().Be(3);
result.Total.Should().Be(42);
result.TotalPages.Should().Be(3);
}

[Fact]
public async Task Handle_LargeDataset_PerformanceUnder1Second()
{
// Arrange - Create 1000 issues
for (int i = 0; i < 1000; i++)
{
	var issue = CreateTestIssueDto($"Issue {i + 1}", $"Description {i + 1}", DateTime.UtcNow.AddMinutes(-i));
	await _repository.CreateAsync(issue);
}

var query = new ListIssuesQuery { Page = 1, PageSize = 20 };

// Act
var stopwatch = System.Diagnostics.Stopwatch.StartNew();
var result = await _handler.Handle(query, CancellationToken.None);
stopwatch.Stop();

// Assert
result.Items.Should().HaveCount(20);
result.Total.Should().Be(1000);
stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // < 1 second
}

[Fact]
public async Task Handle_ConcurrentCreates_ReturnsConsistentResults()
{
// Arrange - Create 20 issues
for (int i = 0; i < 20; i++)
{
	var issue = CreateTestIssueDto($"Issue {i + 1}", $"Description {i + 1}", DateTime.UtcNow.AddMinutes(-i));
	await _repository.CreateAsync(issue);
}

var query = new ListIssuesQuery { Page = 1, PageSize = 20 };

// Act - List while creating new issue
var listTask = _handler.Handle(query, CancellationToken.None);

var newIssue = CreateTestIssueDto("Concurrent Issue", "Created during list");
var createTask = _repository.CreateAsync(newIssue);

await Task.WhenAll(listTask, createTask);

var result = await listTask;

// Assert - List should be consistent (snapshot isolation)
result.Items.Should().HaveCount(20);
result.Total.Should().BeOneOf(20, 21); // Either before or after concurrent create
}
}
