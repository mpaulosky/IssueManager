// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateIssueStatusHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Integration.Tests
// =======================================================
namespace Integration.Handlers;

/// <summary>
/// Integration tests for UpdateIssueStatusHandler with a real MongoDB database.
/// </summary>
[Collection("Integration")]
[ExcludeFromCodeCoverage]
public class UpdateIssueStatusHandlerTests : IAsyncLifetime
{
	private const string MongodbImage = "mongo:latest";
	private const string TestDatabase = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder(MongodbImage)
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
		var created = await _repository.CreateAsync(issueDto, TestContext.Current.CancellationToken);
		var newStatus = new StatusDto(ObjectId.GenerateNewId(), "InProgress", "Issue is in progress", DateTime.UtcNow, null, false, UserDto.Empty);

		if (created.Value != null)
		{
			var command = new UpdateIssueStatusCommand
			{
					IssueId = created.Value.Id,
					Status = newStatus
			};

			// Act
			var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

			// Assert
			result.Success.Should().BeTrue();
			result.Value!.Status.StatusName.Should().Be("InProgress");
		}

		// Verify persistence
		var retrievedResult = await _repository.GetByIdAsync(created.Value!.Id, TestContext.Current.CancellationToken);
		retrievedResult.Should().NotBeNull();
		var retrieved = retrievedResult.Value;
		retrieved?.Status.StatusName.Should().Be("InProgress");
	}

	[Fact]
	public async Task Handle_NonExistingIssue_ReturnsNull()
	{
		// Arrange
		var command = new UpdateIssueStatusCommand
		{
			IssueId = new ObjectId(ObjectId.GenerateNewId().ToString()),
			Status = new StatusDto(ObjectId.GenerateNewId(), "Closed", "Issue is closed", DateTime.UtcNow, null, false, UserDto.Empty)
		};

		// Act
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
	}

	[Fact]
	public async Task Handle_EmptyIssueId_ThrowsValidationException()
	{
		// Arrange
		var command = new UpdateIssueStatusCommand
		{
			IssueId = new ObjectId(""),
			Status = new StatusDto(ObjectId.GenerateNewId(), "InProgress", "Issue is in progress", DateTime.UtcNow, null, false, UserDto.Empty)
		};

		// Act
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
	}

	[Fact]
	public async Task Handle_StatusTransition_OpenToInProgressToClosed_UpdatesCorrectly()
	{
		// Arrange
		var issueDto = CreateTestIssueDto("Status Transition Test", "Testing status transitions");
		var created = await _repository.CreateAsync(issueDto, TestContext.Current.CancellationToken);

		// Act - Transition to InProgress
		var inProgressCommand = new UpdateIssueStatusCommand
		{
			IssueId = new ObjectId(created.Value?.Id.ToString()),
			Status = new StatusDto(ObjectId.GenerateNewId(), "InProgress", "Issue is in progress", DateTime.UtcNow, null, false, UserDto.Empty)
		};
		var inProgressResult = await _handler.Handle(inProgressCommand, TestContext.Current.CancellationToken);

		// Assert InProgress
		inProgressResult.Success.Should().BeTrue();
		inProgressResult.Value!.Status.StatusName.Should().Be("InProgress");

		// Act - Transition to Closed
		var closedCommand = new UpdateIssueStatusCommand
		{
			IssueId = new ObjectId(created.Value?.Id.ToString()),
			Status = new StatusDto(ObjectId.GenerateNewId(), "Closed", "Issue is closed", DateTime.UtcNow, null, false, UserDto.Empty)
		};
		var closedResult = await _handler.Handle(closedCommand, TestContext.Current.CancellationToken);

		// Assert Closed
		closedResult.Success.Should().BeTrue();
		closedResult.Value!.Status.StatusName.Should().Be("Closed");

		// Verify the final state in a database
		var retrievedResult = await _repository.GetByIdAsync(created.Value!.Id, TestContext.Current.CancellationToken);
		retrievedResult.Should().NotBeNull();
		var retrieved = retrievedResult.Value;
		retrieved?.Status.StatusName.Should().Be("Closed");
	}
}
