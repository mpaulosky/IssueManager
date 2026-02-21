using FluentAssertions;
using IssueManager.Api.Data;
using IssueManager.Api.Handlers;
using global::Shared.Domain;
using global::Shared.Exceptions;
using IssueManager.Shared.Validators;
using Testcontainers.MongoDb;

namespace IssueManager.Tests.Integration.Handlers;

/// <summary>
/// Integration tests for DeleteIssueHandler (soft-delete via IsArchived) with real MongoDB database.
/// </summary>
public class DeleteIssueHandlerIntegrationTests : IAsyncLifetime
{
private const string MONGODB_IMAGE = "mongo:8.0";
private const string TEST_DATABASE = "IssueManagerTestDb";
private readonly MongoDbContainer _mongoContainer;

private IIssueRepository _repository = null!;
private DeleteIssueHandler _handler = null!;

public DeleteIssueHandlerIntegrationTests()
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
_handler = new DeleteIssueHandler(_repository, new DeleteIssueValidator());
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
public async Task Handle_ValidIssue_SetsIsArchivedInDatabase()
{
// Arrange - Create an issue
var issue = new Issue(
Id: Guid.NewGuid().ToString(),
Title: "Issue to Delete",
Description: "This will be archived",
Status: IssueStatus.Open,
CreatedAt: DateTime.UtcNow,
UpdatedAt: DateTime.UtcNow)
{
IsArchived = false
};

await _repository.CreateAsync(issue);

var command = new DeleteIssueCommand { Id = issue.Id };

// Act
await _handler.Handle(command, CancellationToken.None);

// Assert - Verify IsArchived is set in database
var dbIssue = await _repository.GetByIdAsync(issue.Id);
dbIssue.Should().NotBeNull();
dbIssue!.IsArchived.Should().BeTrue();
dbIssue.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
}

[Fact]
public async Task Handle_ArchivedIssue_ExcludedFromListByDefault()
{
// Arrange - Create and archive an issue
var issue = new Issue(
Id: Guid.NewGuid().ToString(),
Title: "Issue to Archive",
Description: "Test",
Status: IssueStatus.Open,
CreatedAt: DateTime.UtcNow,
UpdatedAt: DateTime.UtcNow);

await _repository.CreateAsync(issue);

var command = new DeleteIssueCommand { Id = issue.Id };

// Act - Archive the issue
await _handler.Handle(command, CancellationToken.None);

// Assert - GetAll (paginated) should exclude archived issues
var (allIssues, _) = await _repository.GetAllAsync(1, 100);
allIssues.Should().NotContain(i => i.Id == issue.Id);
}

[Fact]
public async Task Handle_NonExistentIssue_ThrowsNotFoundException()
{
// Arrange
var nonExistentId = Guid.NewGuid().ToString();
var command = new DeleteIssueCommand { Id = nonExistentId };

// Act
Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

// Assert
await act.Should().ThrowAsync<NotFoundException>();
}

[Fact]
public async Task Handle_IssueNotDeleted_RecordStillExists()
{
// Arrange - Create an issue
var issue = new Issue(
Id: Guid.NewGuid().ToString(),
Title: "Issue to Archive",
Description: "Should still exist in DB",
Status: IssueStatus.Open,
CreatedAt: DateTime.UtcNow,
UpdatedAt: DateTime.UtcNow);

await _repository.CreateAsync(issue);

var command = new DeleteIssueCommand { Id = issue.Id };

// Act - Soft delete
await _handler.Handle(command, CancellationToken.None);

// Assert - Record should still exist (soft delete)
var dbIssue = await _repository.GetByIdAsync(issue.Id);
dbIssue.Should().NotBeNull();
dbIssue!.Id.Should().Be(issue.Id);
dbIssue.IsArchived.Should().BeTrue();
}

[Fact]
public async Task Handle_AlreadyArchivedIssue_IsIdempotent()
{
// Arrange - Create an already archived issue
var archivedIssue = new Issue(
Id: Guid.NewGuid().ToString(),
Title: "Already Archived",
Description: "Already archived",
Status: IssueStatus.Open,
CreatedAt: DateTime.UtcNow,
UpdatedAt: DateTime.UtcNow.AddHours(-1))
{
IsArchived = true
};

await _repository.CreateAsync(archivedIssue);

var command = new DeleteIssueCommand { Id = archivedIssue.Id };

// Act - Delete already archived issue (should be idempotent)
await _handler.Handle(command, CancellationToken.None);

// Assert - Should still be archived
var dbIssue = await _repository.GetByIdAsync(archivedIssue.Id);
dbIssue.Should().NotBeNull();
dbIssue!.IsArchived.Should().BeTrue();
}

[Fact]
public async Task Handle_MultipleIssues_ArchivesOnlySpecifiedIssue()
{
// Arrange - Create multiple issues
var issue1 = new Issue(
Id: Guid.NewGuid().ToString(),
Title: "Issue 1",
Description: "To be archived",
Status: IssueStatus.Open,
CreatedAt: DateTime.UtcNow,
UpdatedAt: DateTime.UtcNow);

var issue2 = new Issue(
Id: Guid.NewGuid().ToString(),
Title: "Issue 2",
Description: "Should remain active",
Status: IssueStatus.Open,
CreatedAt: DateTime.UtcNow,
UpdatedAt: DateTime.UtcNow);

await _repository.CreateAsync(issue1);
await _repository.CreateAsync(issue2);

var command = new DeleteIssueCommand { Id = issue1.Id };

// Act
await _handler.Handle(command, CancellationToken.None);

// Assert
var dbIssue1 = await _repository.GetByIdAsync(issue1.Id);
var dbIssue2 = await _repository.GetByIdAsync(issue2.Id);

dbIssue1!.IsArchived.Should().BeTrue();
dbIssue2!.IsArchived.Should().BeFalse();
}
}
