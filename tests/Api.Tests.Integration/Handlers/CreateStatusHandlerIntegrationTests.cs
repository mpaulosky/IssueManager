// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateStatusHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Integration
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for CreateStatusHandler with a real MongoDB database.
/// </summary>
[Collection("StatusIntegration")]
[ExcludeFromCodeCoverage]
public class CreateStatusHandlerIntegrationTests
{
	private readonly IStatusRepository _repository;
	private readonly CreateStatusHandler _handler;

	public CreateStatusHandlerIntegrationTests(MongoDbFixture fixture)
	{
		_repository = new StatusRepository(fixture.ConnectionString, $"T{Guid.NewGuid():N}");
		_handler = new CreateStatusHandler(_repository, new CreateStatusValidator());
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
		retrieved.Value!.StatusName.Should().Be("Retrievable Status");
		retrieved.Value.StatusDescription.Should().Be("Test Description");
	}
}
