using Api.Handlers.Issues;

using Shared.Validators;

namespace Integration.Handlers;

/// <summary>
/// Integration tests for UpdateIssueStatusHandler with real MongoDB database.
/// </summary>
[Collection("Integration")]
public class UpdateIssueStatusHandlerTests : IAsyncLifetime
{
	private const string MongodbImage = "mongo:8.2";
	private const string TestDatabase = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder()
			.WithImage(MongodbImage)
			.Build();

	private IIssueRepository _repository = null!;
	private UpdateIssueStatusHandler _handler = null!;

	/// <summary>
	/// Initializes the test container and repository.
	/// </summary>
	public async ValueTask InitializeAsync()
	{
		await _mongoContainer.StartAsync();
		var connectionString = _mongoContainer.GetConnectionString();
		_repository = new IssueRepository(connectionString, TestDatabase);
		_handler = new UpdateIssueStatusHandler(_repository, new UpdateIssueStatusValidator());
	}

	/// <summary>
	/// Disposes the test container.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		await _mongoContainer.StopAsync();
		await _mongoContainer.DisposeAsync();
	}

	private static IssueDto CreateTestIssueDto(string title, string description) =>
		new(ObjectId.GenerateNewId(), title, description, DateTime.UtcNow, null, UserDto.Empty, CategoryDto.Empty, StatusDto.Empty, false, UserDto.Empty, false, false);

	[Fact]
	public async Task Handle_ValidCommand_UpdatesIssueStatus()
	{
		// Arrange
		var issueDto = CreateTestIssueDto("Test Issue", "Test Description");
		var created = await _repository.CreateAsync(issueDto);
		var newStatus = new StatusDto(ObjectId.GenerateNewId(), "InProgress", "Issue is in progress", DateTime.UtcNow, null, false, UserDto.Empty);

		var command = new UpdateIssueStatusCommand
		{
			IssueId = created.Value.Id.ToString(),
			Status = newStatus
		};

		// Act
		var result = await _handler.Handle(command);

		// Assert
		result.Should().NotBeNull();
		result!.Status.StatusName.Should().Be("InProgress");

		// Verify persistence
		var retrievedResult = await _repository.GetByIdAsync(created.Value.Id);
		retrievedResult.Should().NotBeNull();
		var retrieved = retrievedResult.Value;
		retrieved.Status.StatusName.Should().Be("InProgress");
	}

	[Fact]
	public async Task Handle_NonExistingIssue_ReturnsNull()
	{
		// Arrange
		var command = new UpdateIssueStatusCommand
		{
			IssueId = ObjectId.GenerateNewId().ToString(),
			Status = new StatusDto(ObjectId.GenerateNewId(), "Closed", "Issue is closed", DateTime.UtcNow, null, false, UserDto.Empty)
		};

		// Act
		var result = await _handler.Handle(command);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public async Task Handle_EmptyIssueId_ThrowsValidationException()
	{
		// Arrange
		var command = new UpdateIssueStatusCommand
		{
			IssueId = "",
			Status = new StatusDto(ObjectId.GenerateNewId(), "InProgress", "Issue is in progress", DateTime.UtcNow, null, false, UserDto.Empty)
		};

		// Act & Assert
		await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command));
	}

	[Fact]
	public async Task Handle_StatusTransition_OpenToInProgressToClosed_UpdatesCorrectly()
	{
		// Arrange
		var issueDto = CreateTestIssueDto("Status Transition Test", "Testing status transitions");
		var created = await _repository.CreateAsync(issueDto);

		// Act - Transition to InProgress
		var inProgressCommand = new UpdateIssueStatusCommand
		{
			IssueId = created.Value.Id.ToString(),
			Status = new StatusDto(ObjectId.GenerateNewId(), "InProgress", "Issue is in progress", DateTime.UtcNow, null, false, UserDto.Empty)
		};
		var inProgressResult = await _handler.Handle(inProgressCommand);

		// Assert InProgress
		inProgressResult.Should().NotBeNull();
		inProgressResult!.Status.StatusName.Should().Be("InProgress");

		// Act - Transition to Closed
		var closedCommand = new UpdateIssueStatusCommand
		{
			IssueId = created.Value.Id.ToString(),
			Status = new StatusDto(ObjectId.GenerateNewId(), "Closed", "Issue is closed", DateTime.UtcNow, null, false, UserDto.Empty)
		};
		var closedResult = await _handler.Handle(closedCommand);

		// Assert Closed
		closedResult.Should().NotBeNull();
		closedResult!.Status.StatusName.Should().Be("Closed");

		// Verify final state in database
		var retrievedResult = await _repository.GetByIdAsync(created.Value.Id);
		retrievedResult.Should().NotBeNull();
		var retrieved = retrievedResult.Value;
		retrieved.Status.StatusName.Should().Be("Closed");
	}
}
