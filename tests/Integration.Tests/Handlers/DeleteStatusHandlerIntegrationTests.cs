// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteStatusHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Integration.Tests
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for DeleteStatusHandler (soft-delete via Archived) with real MongoDB database.
/// </summary>
[Collection("Integration")]
[ExcludeFromCodeCoverage]
public class DeleteStatusHandlerIntegrationTests : IAsyncLifetime
{
	private const string MongodbImage = "mongo:latest";
	private const string TestDatabase = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder(MongodbImage)
		.Build();

	private IStatusRepository _repository = null!;
	private DeleteStatusHandler _handler = null!;

	/// <summary>
	/// Initializes the test container and repository.
	/// </summary>
	public async ValueTask InitializeAsync()
	{
		await _mongoContainer.StartAsync();
		var connectionString = _mongoContainer.GetConnectionString();
		_repository = new StatusRepository(connectionString, TestDatabase);
		_handler = new DeleteStatusHandler(_repository, new DeleteStatusValidator());
	}

	/// <summary>
	/// Disposes the test container.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		await _mongoContainer.StopAsync();
		await _mongoContainer.DisposeAsync();
	}

	private static StatusDto CreateTestStatusDto(string name, string description = "Test description", bool archived = false) =>
		new(ObjectId.GenerateNewId(), name, description, DateTime.UtcNow, null, archived, UserDto.Empty);

	[Fact]
	public async Task Handle_ValidStatus_SetsArchivedInDatabase()
	{
		// Arrange - Create a status
		var status = CreateTestStatusDto("Status to Delete", "This will be archived");
		var created = await _repository.CreateAsync(status, TestContext.Current.CancellationToken);

		var command = new DeleteStatusCommand { Id = created.Value.Id.ToString() };

		// Act
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Should().BeTrue();

		// Verify Archived is set in database
		var getResult = await _repository.GetByIdAsync(created.Value.Id, TestContext.Current.CancellationToken);
		getResult.Should().NotBeNull();
		getResult.Value.Archived.Should().BeTrue();
	}

	[Fact]
	public async Task Handle_NonExistentStatus_ThrowsNotFoundException()
	{
		// Arrange
		var nonExistentId = ObjectId.GenerateNewId().ToString();
		var command = new DeleteStatusCommand { Id = nonExistentId };

		// Act
		Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>();
	}

	[Fact]
	public async Task Handle_AlreadyArchivedStatus_IsIdempotent()
	{
		// Arrange - Create an already archived status
		var archivedStatus = CreateTestStatusDto("Already Archived", "Already archived", archived: true);
		var created = await _repository.CreateAsync(archivedStatus, TestContext.Current.CancellationToken);

		var command = new DeleteStatusCommand { Id = created.Value.Id.ToString() };

		// Act - Delete already archived status (should be idempotent)
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert - Should still return true
		result.Should().BeTrue();

		var dbStatusResult = await _repository.GetByIdAsync(created.Value.Id, TestContext.Current.CancellationToken);
		dbStatusResult.Should().NotBeNull();
		dbStatusResult.Value.Archived.Should().BeTrue();
	}

	[Fact]
	public async Task Handle_StatusNotDeleted_RecordStillExists()
	{
		// Arrange - Create a status
		var status = CreateTestStatusDto("Status to Archive", "Should still exist in DB");
		var created = await _repository.CreateAsync(status, TestContext.Current.CancellationToken);

		var command = new DeleteStatusCommand { Id = created.Value.Id.ToString() };

		// Act - Soft delete
		await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert - Record should still exist (soft delete)
		var dbStatus = await _repository.GetByIdAsync(created.Value.Id, TestContext.Current.CancellationToken);
		dbStatus.Should().NotBeNull();
		dbStatus.Value.Id.Should().Be(created.Value.Id);
		dbStatus.Value.Archived.Should().BeTrue();
	}
}
