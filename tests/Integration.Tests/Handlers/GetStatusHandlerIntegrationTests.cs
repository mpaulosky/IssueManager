// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     GetStatusHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Integration.Tests
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for GetStatusHandler with real MongoDB database.
/// </summary>
[Collection("Integration")]
[ExcludeFromCodeCoverage]
public class GetStatusHandlerIntegrationTests : IAsyncLifetime
{
	private const string MongodbImage = "mongo:latest";
	private const string TestDatabase = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder(MongodbImage)
		.Build();

	private IStatusRepository _repository = null!;
	private GetStatusHandler _handler = null!;

	/// <summary>
	/// Initializes the test container and repository.
	/// </summary>
	public async ValueTask InitializeAsync()
	{
		await _mongoContainer.StartAsync();
		var connectionString = _mongoContainer.GetConnectionString();
		_repository = new StatusRepository(connectionString, TestDatabase);
		_handler = new GetStatusHandler(_repository);
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
	public async Task Handle_ExistingStatus_ReturnsStatus()
	{
		// Arrange - Create a status
		var status = CreateTestStatusDto("Test Status", "Test Description");
		var created = await _repository.CreateAsync(status, TestContext.Current.CancellationToken);

		var query = new GetStatusQuery(created.Value.Id.ToString());

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Should().NotBeNull();
		result!.StatusName.Should().Be("Test Status");
		result.StatusDescription.Should().Be("Test Description");
	}

	[Fact]
	public async Task Handle_NonExistentStatus_ReturnsNull()
	{
		// Arrange
		var nonExistentId = ObjectId.GenerateNewId().ToString();
		var query = new GetStatusQuery(nonExistentId);

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public async Task Handle_InvalidObjectIdFormat_ReturnsNull()
	{
		// Arrange
		var query = new GetStatusQuery("invalid-id-format");

		// Act
		var result = await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public async Task Handle_EmptyId_ThrowsArgumentException()
	{
		// Arrange
		var query = new GetStatusQuery(string.Empty);

		// Act
		Func<Task> act = async () => await _handler.Handle(query, TestContext.Current.CancellationToken);

		// Assert
		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("Status ID cannot be empty.*");
	}
}
