using FluentAssertions;
using IssueManager.Api.Data;
using global::Shared.Domain;
using Testcontainers.MongoDb;

namespace IssueManager.Tests.Integration.Data;

/// <summary>
/// Integration tests for IssueRepository with pagination, filtering, and soft-delete.
/// </summary>
public class IssueRepositoryTests : IAsyncLifetime
{
private const string MONGODB_IMAGE = "mongo:8.0";
private const string TEST_DATABASE = "IssueManagerTestDb";
private readonly MongoDbContainer _mongoContainer;

private IIssueRepository _repository = null!;

public IssueRepositoryTests()
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
public async Task GetAllAsync_FirstPage_ReturnsCorrectItems()
{
// Arrange - Create 50 issues
for (int i = 0; i < 50; i++)
{
var issue = new Issue(
Id: Guid.NewGuid().ToString(),
Title: $"Issue {i + 1}",
Description: $"Description {i + 1}",
Status: IssueStatus.Open,
CreatedAt: DateTime.UtcNow.AddMinutes(-i),
UpdatedAt: DateTime.UtcNow.AddMinutes(-i));

await _repository.CreateAsync(issue);
}

// Act
var (items, total) = await _repository.GetAllAsync(page: 1, pageSize: 20);

// Assert
items.Should().HaveCount(20);
total.Should().Be(50);
}

[Fact]
public async Task GetAllAsync_SecondPage_ReturnsNextSetOfItems()
{
// Arrange - Create 50 issues
for (int i = 0; i < 50; i++)
{
var issue = new Issue(
Id: Guid.NewGuid().ToString(),
Title: $"Issue {i + 1}",
Description: $"Description {i + 1}",
Status: IssueStatus.Open,
CreatedAt: DateTime.UtcNow.AddMinutes(-i),
UpdatedAt: DateTime.UtcNow.AddMinutes(-i));

await _repository.CreateAsync(issue);
}

// Act
var (page1Items, _) = await _repository.GetAllAsync(page: 1, pageSize: 20);
var (page2Items, _) = await _repository.GetAllAsync(page: 2, pageSize: 20);

// Assert
page2Items.Should().HaveCount(20);
page1Items.Select(i => i.Id).Should().NotIntersectWith(page2Items.Select(i => i.Id)); // No overlap
}

[Fact]
public async Task GetAllAsync_ExcludesArchived_ByDefault()
{
// Arrange - Create 10 issues, archive 3
var issuesToArchive = new List<string>();
for (int i = 0; i < 10; i++)
{
var issue = new Issue(
Id: Guid.NewGuid().ToString(),
Title: $"Issue {i + 1}",
Description: $"Description {i + 1}",
Status: IssueStatus.Open,
CreatedAt: DateTime.UtcNow.AddMinutes(-i),
UpdatedAt: DateTime.UtcNow.AddMinutes(-i));

await _repository.CreateAsync(issue);
if (i < 3)
issuesToArchive.Add(issue.Id);
}

foreach (var id in issuesToArchive)
{
await _repository.ArchiveAsync(id);
}

// Act
var (items, total) = await _repository.GetAllAsync(page: 1, pageSize: 20);

// Assert
items.Should().HaveCount(7); // 10 - 3 archived = 7
total.Should().Be(7);
items.Should().OnlyContain(i => !i.IsArchived);
}

[Fact]
public async Task GetAllAsync_All_IncludesArchivedIssues()
{
// Arrange - Create 10 issues, archive 3
var issuesToArchive = new List<string>();
for (int i = 0; i < 10; i++)
{
var issue = new Issue(
Id: Guid.NewGuid().ToString(),
Title: $"Issue {i + 1}",
Description: $"Description {i + 1}",
Status: IssueStatus.Open,
CreatedAt: DateTime.UtcNow.AddMinutes(-i),
UpdatedAt: DateTime.UtcNow.AddMinutes(-i));

await _repository.CreateAsync(issue);
if (i < 3)
issuesToArchive.Add(issue.Id);
}

foreach (var id in issuesToArchive)
{
await _repository.ArchiveAsync(id);
}

// Act — non-paginated GetAllAsync returns all records
var allIssues = await _repository.GetAllAsync();

// Assert
allIssues.Should().HaveCount(10); // All issues including archived
}

[Fact]
public async Task CountAsync_ReturnsTotalIssueCount()
{
// Arrange - Create 10 issues, archive 3
var issuesToArchive = new List<string>();
for (int i = 0; i < 10; i++)
{
var issue = new Issue(
Id: Guid.NewGuid().ToString(),
Title: $"Issue {i + 1}",
Description: $"Description {i + 1}",
Status: IssueStatus.Open,
CreatedAt: DateTime.UtcNow.AddMinutes(-i),
UpdatedAt: DateTime.UtcNow.AddMinutes(-i));

await _repository.CreateAsync(issue);
if (i < 3)
issuesToArchive.Add(issue.Id);
}

foreach (var id in issuesToArchive)
{
await _repository.ArchiveAsync(id);
}

// Act
var count = await _repository.CountAsync();

// Assert — CountAsync counts all issues regardless of archive status
count.Should().Be(10);
}

[Fact]
public async Task ArchiveAsync_SetsIsArchivedToTrue()
{
// Arrange - Create an issue
var issue = new Issue(
Id: Guid.NewGuid().ToString(),
Title: "Issue to Archive",
Description: "Test",
Status: IssueStatus.Open,
CreatedAt: DateTime.UtcNow,
UpdatedAt: DateTime.UtcNow)
{
IsArchived = false
};

await _repository.CreateAsync(issue);

// Act
var result = await _repository.ArchiveAsync(issue.Id);

// Assert
result.Should().BeTrue();

// Verify in database
var dbIssue = await _repository.GetByIdAsync(issue.Id);
dbIssue.Should().NotBeNull();
dbIssue!.IsArchived.Should().BeTrue();
}

[Fact]
public async Task ArchiveAsync_UpdatesTimestamp()
{
// Arrange - Create an issue
var issue = new Issue(
Id: Guid.NewGuid().ToString(),
Title: "Issue to Archive",
Description: "Test",
Status: IssueStatus.Open,
CreatedAt: DateTime.UtcNow.AddDays(-1),
UpdatedAt: DateTime.UtcNow.AddHours(-2));

await _repository.CreateAsync(issue);

// Act
await _repository.ArchiveAsync(issue.Id);

// Assert
var dbIssue = await _repository.GetByIdAsync(issue.Id);
dbIssue.Should().NotBeNull();
dbIssue!.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
dbIssue.UpdatedAt.Should().BeAfter(issue.UpdatedAt);
}

[Fact]
public async Task ArchiveAsync_DoesNotDeleteRecord()
{
// Arrange - Create an issue
var issue = new Issue(
Id: Guid.NewGuid().ToString(),
Title: "Issue to Archive",
Description: "Should still exist",
Status: IssueStatus.Open,
CreatedAt: DateTime.UtcNow,
UpdatedAt: DateTime.UtcNow);

await _repository.CreateAsync(issue);

// Act - Soft delete (archive)
await _repository.ArchiveAsync(issue.Id);

// Assert - Record still exists
var dbIssue = await _repository.GetByIdAsync(issue.Id);
dbIssue.Should().NotBeNull();
dbIssue!.Id.Should().Be(issue.Id);
dbIssue.IsArchived.Should().BeTrue();
}

[Fact]
public async Task UpdateAsync_NonExistentIssue_ReturnsNull()
{
// Arrange
var nonExistentIssue = new Issue(
Id: Guid.NewGuid().ToString(),
Title: "Non-existent",
Description: "Does not exist",
Status: IssueStatus.Open,
CreatedAt: DateTime.UtcNow,
UpdatedAt: DateTime.UtcNow);

// Act
var result = await _repository.UpdateAsync(nonExistentIssue);

// Assert
result.Should().BeNull();
}

[Fact]
public async Task GetAllAsync_OrdersByCreatedAtDescending()
{
// Arrange - Create issues with specific timestamps
var issue1 = new Issue(
Id: Guid.NewGuid().ToString(),
Title: "Oldest",
Description: "Created first",
Status: IssueStatus.Open,
CreatedAt: DateTime.UtcNow.AddDays(-3),
UpdatedAt: DateTime.UtcNow.AddDays(-3));

var issue2 = new Issue(
Id: Guid.NewGuid().ToString(),
Title: "Middle",
Description: "Created second",
Status: IssueStatus.Open,
CreatedAt: DateTime.UtcNow.AddDays(-2),
UpdatedAt: DateTime.UtcNow.AddDays(-2));

var issue3 = new Issue(
Id: Guid.NewGuid().ToString(),
Title: "Newest",
Description: "Created last",
Status: IssueStatus.Open,
CreatedAt: DateTime.UtcNow.AddDays(-1),
UpdatedAt: DateTime.UtcNow.AddDays(-1));

await _repository.CreateAsync(issue1);
await _repository.CreateAsync(issue2);
await _repository.CreateAsync(issue3);

// Act
var (items, _) = await _repository.GetAllAsync(page: 1, pageSize: 10);

// Assert
items.Should().HaveCount(3);
items[0].Title.Should().Be("Newest"); // Newest first
items[1].Title.Should().Be("Middle");
items[2].Title.Should().Be("Oldest"); // Oldest last
}

[Fact]
public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
{
// Act
var (items, total) = await _repository.GetAllAsync(page: 1, pageSize: 20);

// Assert
items.Should().BeEmpty();
total.Should().Be(0);
}
}
