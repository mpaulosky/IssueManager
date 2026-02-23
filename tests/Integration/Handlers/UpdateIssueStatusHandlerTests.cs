using MongoDB.Bson;
using Shared.DTOs;
using Shared.Validators;

namespace IssueManager.Tests.Integration.Handlers;

/// <summary>
/// Integration tests for UpdateIssueStatusHandler with real MongoDB database.
/// </summary>
public class UpdateIssueStatusHandlerTests : IAsyncLifetime
{
	private const string MONGODB_IMAGE = "mongo:8.0";
	private const string TEST_DATABASE = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer;

	private IIssueRepository _repository = null!;
	private UpdateIssueStatusHandler _handler = null!;

	public UpdateIssueStatusHandlerTests()
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
		_handler = new UpdateIssueStatusHandler(_repository, new UpdateIssueStatusValidator());
	}

	/// <summary>
	/// Disposes the test container.
	/// </summary>
	public async Task DisposeAsync()
	{
		await _mongoContainer.StopAsync();
		await _mongoContainer.DisposeAsync();
	}

	private static IssueDto CreateTestIssueDto(string title, string description) =>
		new(ObjectId.GenerateNewId(), title, description, DateTime.UtcNow, UserDto.Empty, CategoryDto.Empty, StatusDto.Empty);

	[Fact]
	public async Task Handle_ValidCommand_UpdatesIssueStatus()
	{
		// Arrange
		var issueDto = CreateTestIssueDto("Test Issue", "Test Description");
		var created = await _repository.CreateAsync(issueDto);
		var newStatus = new StatusDto("InProgress", "Issue is in progress");

		var command = new UpdateIssueStatusCommand
		{
			IssueId = created.Id.ToString(),
			Status = newStatus
		};

		// Act
		var result = await _handler.Handle(command);

		// Assert
		result.Should().NotBeNull();
		result!.Status.StatusName.Should().Be("InProgress");

		// Verify persistence
		var retrieved = await _repository.GetByIdAsync(created.Id.ToString());
		retrieved!.Status.StatusName.Should().Be("InProgress");
	}

	[Fact]
	public async Task Handle_NonExistingIssue_ReturnsNull()
	{
		// Arrange
		var command = new UpdateIssueStatusCommand
		{
			IssueId = ObjectId.GenerateNewId().ToString(),
			Status = new StatusDto("Closed", "Issue is closed")
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
			Status = new StatusDto("InProgress", "Issue is in progress")
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
			IssueId = created.Id.ToString(),
			Status = new StatusDto("InProgress", "Issue is in progress")
		};
		var inProgressResult = await _handler.Handle(inProgressCommand);

		// Assert InProgress
		inProgressResult.Should().NotBeNull();
		inProgressResult!.Status.StatusName.Should().Be("InProgress");

		// Act - Transition to Closed
		var closedCommand = new UpdateIssueStatusCommand
		{
			IssueId = created.Id.ToString(),
			Status = new StatusDto("Closed", "Issue is closed")
		};
		var closedResult = await _handler.Handle(closedCommand);

		// Assert Closed
		closedResult.Should().NotBeNull();
		closedResult!.Status.StatusName.Should().Be("Closed");

		// Verify final state in database
		var retrieved = await _repository.GetByIdAsync(created.Id.ToString());
		retrieved!.Status.StatusName.Should().Be("Closed");
	}
}
