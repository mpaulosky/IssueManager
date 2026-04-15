// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteStatusHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Integration
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for DeleteStatusHandler (soft-delete via Archived) with a real MongoDB database.
/// </summary>
[Collection("StatusIntegration")]
[ExcludeFromCodeCoverage]
public class DeleteStatusHandlerIntegrationTests
{
	private readonly IStatusRepository _repository;
	private readonly DeleteStatusHandler _handler;

	public DeleteStatusHandlerIntegrationTests(MongoDbFixture fixture)
	{
		fixture.ThrowIfUnavailable();
		_repository = new StatusRepository(fixture.ConnectionString, $"T{Guid.NewGuid():N}");
		_handler = new DeleteStatusHandler(_repository, new DeleteStatusValidator());
	}

	private static StatusDto CreateTestStatusDto(string name, string description = "Test description", bool archived = false) =>
		new(ObjectId.GenerateNewId(), name, description, DateTime.UtcNow, null, archived, UserDto.Empty);

	[Fact]
	public async Task Handle_ValidStatus_SetsArchivedInDatabase()
	{
		// Arrange - Create a status
		var status = CreateTestStatusDto("Status to Delete", "This will be archived");
		var created = await _repository.CreateAsync(status, TestContext.Current.CancellationToken);

		var command = new DeleteStatusCommand { Id = created.Value!.Id };

		// Act
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().BeTrue();

		// Verify Archived is set in the database
		var getResult = await _repository.GetByIdAsync(created.Value.Id, TestContext.Current.CancellationToken);
		getResult.Should().NotBeNull();
		getResult.Value?.Archived.Should().BeTrue();
	}

	[Fact]
	public async Task Handle_NonExistentStatus_ReturnsNotFoundFailure()
	{
		// Arrange
		var nonExistentId = ObjectId.GenerateNewId();
		var command = new DeleteStatusCommand { Id = nonExistentId };

		// Act
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.NotFound);
	}

	[Fact]
	public async Task Handle_AlreadyArchivedStatus_IsIdempotent()
	{
		// Arrange - Create an already archived status
		var archivedStatus = CreateTestStatusDto("Already Archived", "Already archived", archived: true);
		var created = await _repository.CreateAsync(archivedStatus, TestContext.Current.CancellationToken);

		var command = new DeleteStatusCommand { Id = created.Value!.Id };

		// Act - Delete already archived status (should be idempotent)
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert - Should still return true
		result.Success.Should().BeTrue();
		result.Value.Should().BeTrue();

		var dbStatusResult = await _repository.GetByIdAsync(created.Value.Id, TestContext.Current.CancellationToken);
		dbStatusResult.Should().NotBeNull();
		dbStatusResult.Value?.Archived.Should().BeTrue();
	}

	[Fact]
	public async Task Handle_StatusNotDeleted_RecordStillExists()
	{
		// Arrange - Create a status
		var status = CreateTestStatusDto("Status to Archive", "Should still exist in DB");
		var created = await _repository.CreateAsync(status, TestContext.Current.CancellationToken);

		var command = new DeleteStatusCommand { Id = created.Value!.Id };

		// Act - Soft delete
		await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert - Record should still exist (soft delete)
		var dbStatus = await _repository.GetByIdAsync(created.Value.Id, TestContext.Current.CancellationToken);
		dbStatus.Should().NotBeNull();
		dbStatus.Value?.Id.Should().Be(created.Value.Id);
		dbStatus.Value?.Archived.Should().BeTrue();
	}

	[Fact]
	public async Task Handle_CreatedAndDeletedStatus_NotReturnedInList()
	{
		// Arrange - Create a status via repository
		var status = CreateTestStatusDto("Status for List Test", "Will be archived");
		var created = await _repository.CreateAsync(status, TestContext.Current.CancellationToken);
		created.Value.Should().NotBeNull();

		var command = new DeleteStatusCommand { Id = created.Value!.Id };

		// Act - Archive the status
		await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert - GetAll (paginated) should exclude archived statuses
		var result = await _repository.GetAllAsync(1, 100, TestContext.Current.CancellationToken);
		var allStatuses = result.Value.Items;
		allStatuses.Should().NotContain(s => s.Id == created.Value.Id);
	}
}
