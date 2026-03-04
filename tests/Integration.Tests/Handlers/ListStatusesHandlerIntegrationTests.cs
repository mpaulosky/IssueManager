// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ListStatusesHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Integration.Tests
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for ListStatusesHandler with a real MongoDB database.
/// </summary>
[Collection("Integration")]
[ExcludeFromCodeCoverage]
public class ListStatusesHandlerIntegrationTests : IAsyncLifetime
{
	private const string MongodbImage = "mongo:latest";
	private const string TestDatabase = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder(MongodbImage)
		.Build();

	private IStatusRepository _repository = null!;
	private ListStatusesHandler _handler = null!;

	/// <summary>
	/// Initializes the test container and repository.
	/// </summary>
	public async ValueTask InitializeAsync()
	{
		await _mongoContainer.StartAsync();
		var connectionString = _mongoContainer.GetConnectionString();
		_repository = new StatusRepository(connectionString, TestDatabase);
		_handler = new ListStatusesHandler(_repository);
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
	public async Task Handle_EmptyDatabase_ReturnsEmptyList()
	{
		// Arrange - No statuses in database

		// Act
		var result = await _handler.Handle(TestContext.Current.CancellationToken);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_WithStatuses_ReturnsAll()
	{
		// Arrange - Create 3 statuses
		var status1 = CreateTestStatusDto("Status 1", "Description 1");
		var status2 = CreateTestStatusDto("Status 2", "Description 2");
		var status3 = CreateTestStatusDto("Status 3", "Description 3");

		await _repository.CreateAsync(status1, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(status2, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(status3, TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.Handle(TestContext.Current.CancellationToken);

		// Assert
		IEnumerable<StatusDto> statusDtos = result.ToList();
		statusDtos.Should().HaveCount(3);
		statusDtos.Should().Contain(s => s.StatusName == "Status 1");
		statusDtos.Should().Contain(s => s.StatusName == "Status 2");
		statusDtos.Should().Contain(s => s.StatusName == "Status 3");
	}

	[Fact]
	public async Task Handle_MultipleStatuses_ReturnsAll_IncludingArchived()
	{
		// Arrange - Create 5 statuses, archive 2
		var statusesToCreate = new List<StatusDto>();
		for (int i = 1; i <= 5; i++)
		{
			var status = CreateTestStatusDto($"Status {i}", $"Description {i}");
			var created = await _repository.CreateAsync(status, TestContext.Current.CancellationToken);
			statusesToCreate.Add(created.Value!);
		}

		// Archive first 2 statuses
		await _repository.ArchiveAsync(statusesToCreate[0].Id, TestContext.Current.CancellationToken);
		await _repository.ArchiveAsync(statusesToCreate[1].Id, TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.Handle(TestContext.Current.CancellationToken);

		// Assert
		result.Should().HaveCount(5); // GetAllAsync returns all including archived
	}
}
