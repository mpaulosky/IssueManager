// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateStatusHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Integration.Tests
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for CreateStatusHandler with real MongoDB database.
/// </summary>
[Collection("Integration")]
[ExcludeFromCodeCoverage]
public class CreateStatusHandlerIntegrationTests : IAsyncLifetime
{
	private const string MongodbImage = "mongo:latest";
	private const string TestDatabase = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder(MongodbImage)
		.Build();

	private IStatusRepository _repository = null!;
	private CreateStatusHandler _handler = null!;

	/// <summary>
	/// Initializes the test container and repository.
	/// </summary>
	public async ValueTask InitializeAsync()
	{
		await _mongoContainer.StartAsync();
		var connectionString = _mongoContainer.GetConnectionString();
		_repository = new StatusRepository(connectionString, TestDatabase);
		_handler = new CreateStatusHandler(_repository, new CreateStatusValidator());
	}

	/// <summary>
	/// Disposes the test container.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		await _mongoContainer.StopAsync();
		await _mongoContainer.DisposeAsync();
	}

	[Fact]
	public async Task Handle_ValidCommand_CreatesStatus()
	{
		// Arrange
		var command = new CreateStatusCommand
		{
			StatusName = "New Status",
			StatusDescription = "New Description"
		};

		// Act
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Should().NotBeNull();
		result.StatusName.Should().Be("New Status");
		result.StatusDescription.Should().Be("New Description");
		result.Id.Should().NotBe(ObjectId.Empty);
		result.Archived.Should().BeFalse();
	}

	[Fact]
	public async Task Handle_InvalidCommand_ThrowsValidationException()
	{
		// Arrange - Empty status name is invalid
		var command = new CreateStatusCommand
		{
			StatusName = string.Empty,
			StatusDescription = "Description"
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		await act.Should().ThrowAsync<ValidationException>();
	}

	[Fact]
	public async Task Handle_CreatedStatus_CanBeRetrieved()
	{
		// Arrange
		var command = new CreateStatusCommand
		{
			StatusName = "Retrievable Status",
			StatusDescription = "Test Description"
		};

		// Act - Create status
		var created = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert - Verify it can be retrieved
		var retrieved = await _repository.GetByIdAsync(created.Id, TestContext.Current.CancellationToken);
		retrieved.Should().NotBeNull();
		retrieved.Value.StatusName.Should().Be("Retrievable Status");
		retrieved.Value.StatusDescription.Should().Be("Test Description");
	}
}
