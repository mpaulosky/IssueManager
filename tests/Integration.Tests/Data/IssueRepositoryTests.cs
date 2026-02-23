using FluentAssertions;
using Api.Data;
using MongoDB.Bson;
using Shared.DTOs;
using Testcontainers.MongoDb;

namespace Tests.Integration.Data;

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

private static IssueDto CreateTestIssueDto(string title, string description, DateTime? dateCreated = null) =>
new(
	ObjectId.GenerateNewId(),
	title,
	description,
	dateCreated ?? DateTime.UtcNow,
	UserDto.Empty,
	CategoryDto.Empty,
	StatusDto.Empty);

[Fact]
public async Task GetAllAsync_FirstPage_ReturnsCorrectItems()
{
// Arrange - Create 50 issues
for (int i = 0; i < 50; i++)
{
	var issue = CreateTestIssueDto($"Issue {i + 1}", $"Description {i + 1}", DateTime.UtcNow.AddMinutes(-i));
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
	var issue = CreateTestIssueDto($"Issue {i + 1}", $"Description {i + 1}", DateTime.UtcNow.AddMinutes(-i));
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
	var issue = CreateTestIssueDto($"Issue {i + 1}", $"Description {i + 1}", DateTime.UtcNow.AddMinutes(-i));
	var created = await _repository.CreateAsync(issue);
	if (i < 3)
		issuesToArchive.Add(created.Id.ToString());
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
items.Should().OnlyContain(i => !i.Archived);
}

[Fact]
public async Task GetAllAsync_All_IncludesArchivedIssues()
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
	var issue = CreateTestIssueDto($"Issue {i + 1}", $"Description {i + 1}", DateTime.UtcNow.AddMinutes(-i));
	var created = await _repository.CreateAsync(issue);
	if (i < 3)
		issuesToArchive.Add(created.Id.ToString());
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
public async Task ArchiveAsync_SetsArchivedToTrue()
{
// Arrange - Create an issue
var issue = CreateTestIssueDto("Issue to Archive", "Test");
var created = await _repository.CreateAsync(issue);

// Act
var result = await _repository.ArchiveAsync(created.Id.ToString());

// Assert
result.Should().BeTrue();

// Verify in database
var dbIssue = await _repository.GetByIdAsync(created.Id.ToString());
dbIssue.Should().NotBeNull();
dbIssue!.Archived.Should().BeTrue();
}

[Fact]
public async Task ArchiveAsync_DoesNotDeleteRecord()
{
// Arrange - Create an issue
var issue = CreateTestIssueDto("Issue to Archive", "Should still exist");
var created = await _repository.CreateAsync(issue);

// Act - Soft delete (archive)
await _repository.ArchiveAsync(created.Id.ToString());

// Assert - Record still exists
var dbIssue = await _repository.GetByIdAsync(created.Id.ToString());
dbIssue.Should().NotBeNull();
dbIssue!.Id.Should().Be(created.Id);
dbIssue.Archived.Should().BeTrue();
}

[Fact]
public async Task UpdateAsync_NonExistentIssue_ReturnsNull()
{
// Arrange
var nonExistentIssue = CreateTestIssueDto("Non-existent", "Does not exist");

// Act
var result = await _repository.UpdateAsync(nonExistentIssue);

// Assert
result.Should().BeNull();
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
