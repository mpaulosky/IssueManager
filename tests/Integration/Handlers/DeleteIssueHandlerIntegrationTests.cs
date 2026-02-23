using FluentAssertions;
using Api.Data;
using Api.Handlers;
using MongoDB.Bson;
using Shared.DTOs;
using Shared.Exceptions;
using Shared.Validators;
using Testcontainers.MongoDb;

namespace IssueManager.Tests.Integration.Handlers;

/// <summary>
/// Integration tests for DeleteIssueHandler (soft-delete via Archived) with real MongoDB database.
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

private static IssueDto CreateTestIssueDto(string title, string description, bool archived = false) =>
new(ObjectId.GenerateNewId(), title, description, DateTime.UtcNow, UserDto.Empty, CategoryDto.Empty, StatusDto.Empty, archived);

[Fact]
public async Task Handle_ValidIssue_SetsArchivedInDatabase()
{
// Arrange - Create an issue
var issue = CreateTestIssueDto("Issue to Delete", "This will be archived");
var created = await _repository.CreateAsync(issue);

var command = new DeleteIssueCommand { Id = created.Id.ToString() };

// Act
await _handler.Handle(command, CancellationToken.None);

// Assert - Verify Archived is set in database
var dbIssue = await _repository.GetByIdAsync(created.Id.ToString());
dbIssue.Should().NotBeNull();
dbIssue!.Archived.Should().BeTrue();
}

[Fact]
public async Task Handle_ArchivedIssue_ExcludedFromListByDefault()
{
// Arrange - Create and archive an issue
var issue = CreateTestIssueDto("Issue to Archive", "Test");
var created = await _repository.CreateAsync(issue);

var command = new DeleteIssueCommand { Id = created.Id.ToString() };

// Act - Archive the issue
await _handler.Handle(command, CancellationToken.None);

// Assert - GetAll (paginated) should exclude archived issues
var (allIssues, _) = await _repository.GetAllAsync(1, 100);
allIssues.Should().NotContain(i => i.Id == created.Id);
}

[Fact]
public async Task Handle_NonExistentIssue_ThrowsNotFoundException()
{
// Arrange
var nonExistentId = ObjectId.GenerateNewId().ToString();
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
var issue = CreateTestIssueDto("Issue to Archive", "Should still exist in DB");
var created = await _repository.CreateAsync(issue);

var command = new DeleteIssueCommand { Id = created.Id.ToString() };

// Act - Soft delete
await _handler.Handle(command, CancellationToken.None);

// Assert - Record should still exist (soft delete)
var dbIssue = await _repository.GetByIdAsync(created.Id.ToString());
dbIssue.Should().NotBeNull();
dbIssue!.Id.Should().Be(created.Id);
dbIssue.Archived.Should().BeTrue();
}

[Fact]
public async Task Handle_AlreadyArchivedIssue_IsIdempotent()
{
// Arrange - Create an already archived issue
var archivedIssue = CreateTestIssueDto("Already Archived", "Already archived", archived: true);
var created = await _repository.CreateAsync(archivedIssue);

var command = new DeleteIssueCommand { Id = created.Id.ToString() };

// Act - Delete already archived issue (should be idempotent)
await _handler.Handle(command, CancellationToken.None);

// Assert - Should still be archived
var dbIssue = await _repository.GetByIdAsync(created.Id.ToString());
dbIssue.Should().NotBeNull();
dbIssue!.Archived.Should().BeTrue();
}

[Fact]
public async Task Handle_MultipleIssues_ArchivesOnlySpecifiedIssue()
{
// Arrange - Create multiple issues
var issue1 = CreateTestIssueDto("Issue 1", "To be archived");
var issue2 = CreateTestIssueDto("Issue 2", "Should remain active");

var created1 = await _repository.CreateAsync(issue1);
var created2 = await _repository.CreateAsync(issue2);

var command = new DeleteIssueCommand { Id = created1.Id.ToString() };

// Act
await _handler.Handle(command, CancellationToken.None);

// Assert
var dbIssue1 = await _repository.GetByIdAsync(created1.Id.ToString());
var dbIssue2 = await _repository.GetByIdAsync(created2.Id.ToString());

dbIssue1!.Archived.Should().BeTrue();
dbIssue2!.Archived.Should().BeFalse();
}
}
