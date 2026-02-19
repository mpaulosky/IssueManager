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

	[Fact]
	public async Task Handle_ValidCommand_UpdatesIssueStatus()
	{
		// Arrange
		var issue = Issue.Create("Test Issue", "Test Description");
		await _repository.CreateAsync(issue);

		var command = new UpdateIssueStatusCommand
		{
			IssueId = issue.Id,
			Status = IssueStatus.InProgress
		};

		// Act
		var result = await _handler.Handle(command);

		// Assert
		result.Should().NotBeNull();
		result!.Status.Should().Be(IssueStatus.InProgress);
		result.UpdatedAt.Should().BeAfter(issue.UpdatedAt);

		// Verify persistence
		var retrieved = await _repository.GetByIdAsync(issue.Id);
		retrieved!.Status.Should().Be(IssueStatus.InProgress);
	}

	[Fact]
	public async Task Handle_NonExistingIssue_ReturnsNull()
	{
		// Arrange
		var command = new UpdateIssueStatusCommand
		{
			IssueId = "non-existing-id",
			Status = IssueStatus.Closed
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
			Status = IssueStatus.InProgress
		};

		// Act & Assert
		await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command));
	}

	[Fact]
	public async Task Handle_StatusTransition_OpenToInProgressToClosed_UpdatesCorrectly()
	{
		// Arrange
		var issue = Issue.Create("Status Transition Test", "Testing status transitions");
		await _repository.CreateAsync(issue);

		// Act - Transition to InProgress
		var inProgressCommand = new UpdateIssueStatusCommand
		{
			IssueId = issue.Id,
			Status = IssueStatus.InProgress
		};
		var inProgressResult = await _handler.Handle(inProgressCommand);

		// Assert InProgress
		inProgressResult.Should().NotBeNull();
		inProgressResult!.Status.Should().Be(IssueStatus.InProgress);

		// Act - Transition to Closed
		var closedCommand = new UpdateIssueStatusCommand
		{
			IssueId = issue.Id,
			Status = IssueStatus.Closed
		};
		var closedResult = await _handler.Handle(closedCommand);

		// Assert Closed
		closedResult.Should().NotBeNull();
		closedResult!.Status.Should().Be(IssueStatus.Closed);
		closedResult.UpdatedAt.Should().BeAfter(inProgressResult.UpdatedAt);

		// Verify final state in database
		var retrieved = await _repository.GetByIdAsync(issue.Id);
		retrieved!.Status.Should().Be(IssueStatus.Closed);
	}
}
