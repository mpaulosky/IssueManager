// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     GetStatusHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Integration
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for GetStatusHandler with a real MongoDB database.
/// </summary>
[Collection("StatusIntegration")]
[ExcludeFromCodeCoverage]
public class GetStatusHandlerIntegrationTests
{
	private readonly IStatusRepository _repository;
	private readonly GetStatusHandler _handler;

	public GetStatusHandlerIntegrationTests(MongoDbFixture fixture)
	{
		fixture.ThrowIfUnavailable();
		_repository = new StatusRepository(fixture.ConnectionString, $"T{Guid.NewGuid():N}");
		_handler = new GetStatusHandler(_repository);
	}

	private static StatusDto CreateTestStatusDto(string name, string description = "Test description") =>
		new(ObjectId.GenerateNewId(), name, description, DateTime.UtcNow, null, false, UserDto.Empty);

	[Fact]
	public async Task Handle_ExistingStatus_ReturnsStatus()
	{
		// Arrange - Create a status
		var status = CreateTestStatusDto("Test Status", "Test Description");
		var created = await _repository.CreateAsync(status, TestContext.Current.CancellationToken);

		var query = new GetStatusQuery(created.Value!.Id);

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
		result.Value!.StatusName.Should().Be("Test Status");
		result.Value!.StatusDescription.Should().Be("Test Description");
	}

	[Fact]
	public async Task Handle_NonExistentStatus_ReturnsNull()
	{
		// Arrange
		var nonExistentId = ObjectId.GenerateNewId();
		var query = new GetStatusQuery(nonExistentId);

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
	}

	[Fact]
	public async Task Handle_EmptyObjectId_ReturnsNull()
	{
		// Arrange
		var query = new GetStatusQuery(ObjectId.Empty);

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
	}
}
