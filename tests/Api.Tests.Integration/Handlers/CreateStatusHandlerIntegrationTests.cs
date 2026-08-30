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
/// Integration tests for the generic create path (TaxonomyCrudHandler via StatusTaxonomyAdapter), with a real MongoDB database.
/// </summary>
[Collection("StatusIntegration")]
[ExcludeFromCodeCoverage]
public class CreateStatusHandlerIntegrationTests
{
	private readonly IStatusRepository _repository;
	private readonly TaxonomyCrudHandler<StatusDto, CreateStatusCommand, UpdateStatusCommand> _handler;

	public CreateStatusHandlerIntegrationTests(MongoDbFixture fixture)
	{
		fixture.ThrowIfUnavailable();
		_repository = new StatusRepository(fixture.ConnectionString, $"T{Guid.NewGuid():N}");
		_handler = new TaxonomyCrudHandler<StatusDto, CreateStatusCommand, UpdateStatusCommand>(_repository, StatusTaxonomyAdapter.Instance);
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
		var result = await _handler.HandleCreate(command, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
		result.Value!.StatusName.Should().Be("New Status");
		result.Value!.StatusDescription.Should().Be("New Description");
		result.Value!.Id.Should().NotBe(ObjectId.Empty);
		result.Value!.Archived.Should().BeFalse();
	}

	[Fact]
	public async Task Handle_InvalidCommand_ReturnsValidationFailure()
	{
		// Arrange - Empty status name is invalid
		var command = new CreateStatusCommand
		{
			StatusName = string.Empty,
			StatusDescription = "Description"
		};

		// Act
		var result = await _handler.HandleCreate(command, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
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
		var created = await _handler.HandleCreate(command, TestContext.Current.CancellationToken);

		// Assert - Verify it can be retrieved
		var retrieved = await _repository.GetByIdAsync(created.Value!.Id, TestContext.Current.CancellationToken);
		retrieved.Should().NotBeNull();
		retrieved.Value!.StatusName.Should().Be("Retrievable Status");
		retrieved.Value.StatusDescription.Should().Be("Test Description");
	}
}
