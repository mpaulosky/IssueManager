using Api.Handlers.Issues;

using Shared.Exceptions;
using Shared.Validators;

namespace Integration.Handlers;

/// <summary>
/// Integration tests for UpdateIssueHandler with a real MongoDB database.
/// </summary>
[Collection("Integration")]
public class UpdateIssueHandlerIntegrationTests : IAsyncLifetime
{
private const string MongodbImage = "mongo:8.2";
private const string TestDatabase = "IssueManagerTestDb";
private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder()
		.WithImage(MongodbImage)
		.Build();

private IIssueRepository _repository = null!;
private UpdateIssueHandler _handler = null!;

/// <summary>
/// Initializes the test container and repository.
/// </summary>
public async ValueTask InitializeAsync()
{
await _mongoContainer.StartAsync();
var connectionString = _mongoContainer.GetConnectionString();
_repository = new IssueRepository(connectionString, TestDatabase);
_handler = new UpdateIssueHandler(_repository, new UpdateIssueValidator());
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
public async Task Handle_ValidUpdate_UpdatesIssueInDatabase()
{
// Arrange - Create an issue first
var originalIssue = CreateTestIssueDto("Original Title", "Original Description");
var created = await _repository.CreateAsync(originalIssue);

var command = new UpdateIssueCommand
{
Id = created.Value.Id.ToString(),
Title = "Updated Title",
Description = "Updated Description"
};

// Act
var result = await _handler.Handle(command, CancellationToken.None);

// Assert
result.Should().NotBeNull();
result.Id.Should().Be(created.Value.Id);
result.Title.Should().Be("Updated Title");
result.Description.Should().Be("Updated Description");

// Verify in database
var dbIssueResult = await _repository.GetByIdAsync(created.Value.Id);
dbIssueResult.Should().NotBeNull();
var dbIssue = dbIssueResult.Value;
dbIssue.Title.Should().Be("Updated Title");
dbIssue.Description.Should().Be("Updated Description");
}

[Fact]
public async Task Handle_AtomicUpdate_TitleAndDescriptionBothUpdate()
{
// Arrange
var originalIssue = CreateTestIssueDto("Original Title", "Original Description");
var created = await _repository.CreateAsync(originalIssue);

var command = new UpdateIssueCommand
{
Id = created.Value.Id.ToString(),
Title = "New Title",
Description = "New Description"
};

// Act
var result = await _handler.Handle(command, CancellationToken.None);

// Assert - Both fields should be updated atomically
var dbIssueResult = await _repository.GetByIdAsync(created.Value.Id);
dbIssueResult.Should().NotBeNull();
var dbIssue = dbIssueResult.Value;
dbIssue.Title.Should().Be("New Title");
dbIssue.Description.Should().Be("New Description");
}

[Fact]
public async Task Handle_NonExistentIssue_ThrowsNotFoundException()
{
// Arrange
var nonExistentId = ObjectId.GenerateNewId().ToString();
var command = new UpdateIssueCommand
{
Id = nonExistentId,
Title = "Title",
Description = "Description"
};

// Act
Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

// Assert
await act.Should().ThrowAsync<NotFoundException>();
}

[Fact]
public async Task Handle_ConcurrentUpdates_LastWriteWins()
{
// Arrange - Create an issue
var issue = CreateTestIssueDto("Original Title", "Original Description");
var created = await _repository.CreateAsync(issue);

var command1 = new UpdateIssueCommand
{
Id = created.Value.Id.ToString(),
Title = "First Update",
Description = "First Description"
};

var command2 = new UpdateIssueCommand
{
Id = created.Value.Id.ToString(),
Title = "Second Update",
Description = "Second Description"
};

// Act - Simulate sequential updates
await _handler.Handle(command1, CancellationToken.None);
await Task.Delay(100); // Small delay to ensure different ordering
await _handler.Handle(command2, CancellationToken.None);

// Assert - Last write wins
var dbIssueResult = await _repository.GetByIdAsync(created.Value.Id);
dbIssueResult.Should().NotBeNull();
var dbIssue = dbIssueResult.Value;
dbIssue.Title.Should().Be("Second Update");
dbIssue.Description.Should().Be("Second Description");
}

[Fact]
public async Task Handle_ArchivedIssue_ThrowsConflictException()
{
// Arrange - Create and archive an issue
var archivedIssue = CreateTestIssueDto("Archived Issue", "This is archived", archived: true);
var created = await _repository.CreateAsync(archivedIssue);

var command = new UpdateIssueCommand
{
Id = created.Value.Id.ToString(),
Title = "Attempt Update",
Description = "Should fail"
};

// Act
Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

// Assert
await act.Should().ThrowAsync<ConflictException>();

// Verify the issue wasn't updated
var dbIssueResult = await _repository.GetByIdAsync(created.Value.Id);
var dbIssue = dbIssueResult.Value;
dbIssue.Title.Should().Be("Archived Issue");
}
}
