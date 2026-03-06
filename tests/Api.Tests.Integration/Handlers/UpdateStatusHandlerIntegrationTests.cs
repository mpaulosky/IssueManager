// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateStatusHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Integration
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for UpdateStatusHandler with a real MongoDB database.
/// </summary>
[Collection("Integration")]
[ExcludeFromCodeCoverage]
public class UpdateStatusHandlerIntegrationTests : IAsyncLifetime
{
	private const string MongodbImage = "mongo:latest";
	private const string TestDatabase = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder(MongodbImage)
		.Build();

	private IStatusRepository _repository = null!;
	private UpdateStatusHandler _handler = null!;

	/// <summary>
	/// Initializes the test container and repository.
	/// </summary>
	public async ValueTask InitializeAsync()
	{
		await _mongoContainer.StartAsync();
		var connectionString = _mongoContainer.GetConnectionString();
		_repository = new StatusRepository(connectionString, TestDatabase);
		_handler = new UpdateStatusHandler(_repository, new UpdateStatusValidator());
	}

	/// <summary>
	/// Disposes the test container.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		await _mongoContainer.StopAsync();
		await _mongoContainer.DisposeAsync();
	}

	private static StatusDto CreateTestStatusDto(string name, string description = "Test description") =>
		new(ObjectId.GenerateNewId(), name, description, DateTime.UtcNow, null, false, UserDto.Empty);

	[Fact]
	public async Task Handle_ExistingStatus_UpdatesSuccessfully()
	{
		// Arrange - Create a status
		var status = CreateTestStatusDto("Original Name", "Original Description");
		var created = await _repository.CreateAsync(status, TestContext.Current.CancellationToken);

		var command = new UpdateStatusCommand
		{
			Id = created.Value!.Id,
			StatusName = "Updated Name",
			StatusDescription = "Updated Description"
		};

		// Act
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
		result.Value!.StatusName.Should().Be("Updated Name");
		result.Value!.StatusDescription.Should().Be("Updated Description");
		result.Value!.Id.Should().Be(created.Value.Id);
	}

	[Fact]
	public async Task Handle_NonExistentStatus_ThrowsNotFoundException()
	{
		// Arrange
		ObjectId nonExistentId = ObjectId.GenerateNewId();
		var command = new UpdateStatusCommand
		{
			Id = nonExistentId,
			StatusName = "Updated Name",
			StatusDescription = "Updated Description"
		};

		// Act
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
	}

	[Fact]
	public async Task Handle_InvalidCommand_ThrowsValidationException()
	{
		// Arrange - Empty name is invalid
		var status = CreateTestStatusDto("Test Status", "Test Description");
		var created = await _repository.CreateAsync(status, TestContext.Current.CancellationToken);

		var command = new UpdateStatusCommand
		{
			Id = created.Value!.Id,
			StatusName = string.Empty,
			StatusDescription = "Updated Description"
		};

		// Act
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
	}
}
