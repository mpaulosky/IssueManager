using Api.Handlers.Issues;

using Shared.Exceptions;
using Shared.Validators;

namespace Integration.Handlers;

/// <summary>
/// Integration tests for DeleteIssueHandler (soft-delete via Archived) with real MongoDB database.
/// </summary>
[Collection("Integration")]
public class DeleteIssueHandlerIntegrationTests : IAsyncLifetime
{
private const string MongodbImage = "mongo:latest";
private const string TestDatabase = "IssueManagerTestDb";
private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder(MongodbImage)
		.Build();

private IIssueRepository _repository = null!;
private DeleteIssueHandler _handler = null!;

/// <summary>
/// Initializes the test container and repository.
/// </summary>
public async ValueTask InitializeAsync()
{
await _mongoContainer.StartAsync();
var connectionString = _mongoContainer.GetConnectionString();
_repository = new IssueRepository(connectionString, TestDatabase);
_handler = new DeleteIssueHandler(_repository, new DeleteIssueValidator());
}

/// <summary>
/// Disposes the test container.
/// </summary>
public async ValueTask DisposeAsync()
{
await _mongoContainer.StopAsync();
await _mongoContainer.DisposeAsync();
}

private static IssueDto CreateTestIssueDto(string title, string description, bool archived = false) =>
new(ObjectId.GenerateNewId(), title, description, DateTime.UtcNow, null, UserDto.Empty, CategoryDto.Empty, StatusDto.Empty, archived, UserDto.Empty, false, false);

[Fact]
public async Task Handle_ValidIssue_SetsArchivedInDatabase()
{
// Arrange - Create an issue
var issue = CreateTestIssueDto("Issue to Delete", "This will be archived");
var created = await _repository.CreateAsync(issue);

var command = new DeleteIssueCommand { Id = created.Value.Id.ToString() };

// Act
await _handler.Handle(command, CancellationToken.None);

// Assert - Verify Archived is set in database
var getResult = await _repository.GetByIdAsync(created.Value.Id);
getResult.Should().NotBeNull();
var dbIssue = getResult.Value;
dbIssue.Archived.Should().BeTrue();
}

[Fact]
public async Task Handle_ArchivedIssue_ExcludedFromListByDefault()
{
// Arrange - Create and archive an issue
var issue = CreateTestIssueDto("Issue to Archive", "Test");
var created = await _repository.CreateAsync(issue);

var command = new DeleteIssueCommand { Id = created.Value.Id.ToString() };

// Act - Archive the issue
await _handler.Handle(command, CancellationToken.None);

// Assert - GetAll (paginated) should exclude archived issues
var result = await _repository.GetAllAsync(1, 100);
var allIssues = result.Value.Items;
allIssues.Should().NotContain(i => i.Id == created.Value.Id);
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

var command = new DeleteIssueCommand { Id = created.Value.Id.ToString() };

// Act - Soft delete
await _handler.Handle(command, CancellationToken.None);

// Assert - Record should still exist (soft delete)
var dbIssue = await _repository.GetByIdAsync(created.Value.Id);
var getResult = dbIssue;
getResult.Should().NotBeNull();
var dto = getResult.Value;
dto.Id.Should().Be(created.Value.Id);
dto.Archived.Should().BeTrue();
}

[Fact]
public async Task Handle_AlreadyArchivedIssue_IsIdempotent()
{
// Arrange - Create an already archived issue
var archivedIssue = CreateTestIssueDto("Already Archived", "Already archived", archived: true);
var created = await _repository.CreateAsync(archivedIssue);

var command = new DeleteIssueCommand { Id = created.Value.Id.ToString() };

// Act - Delete already archived issue (should be idempotent)
await _handler.Handle(command, CancellationToken.None);

// Assert - Should still be archived
var dbIssueResult = await _repository.GetByIdAsync(created.Value.Id);
dbIssueResult.Should().NotBeNull();
var dbIssue = dbIssueResult.Value;
dbIssue.Archived.Should().BeTrue();
}

[Fact]
public async Task Handle_MultipleIssues_ArchivesOnlySpecifiedIssue()
{
// Arrange - Create multiple issues
var issue1 = CreateTestIssueDto("Issue 1", "To be archived");
var issue2 = CreateTestIssueDto("Issue 2", "Should remain active");

var created1 = await _repository.CreateAsync(issue1);
var created2 = await _repository.CreateAsync(issue2);

var command = new DeleteIssueCommand { Id = created1.Value.Id.ToString() };

// Act
await _handler.Handle(command, CancellationToken.None);

// Assert
var getResult1 = await _repository.GetByIdAsync(created1.Value.Id);
var getResult2 = await _repository.GetByIdAsync(created2.Value.Id);

getResult1.Should().NotBeNull();

var dto = getResult1.Value;

dto.Archived.Should().BeTrue();
getResult2.Value.Archived.Should().BeFalse();
}
}
