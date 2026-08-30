// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     StatusRepositoryTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Integration
// =======================================================

namespace Integration.Data;

/// <summary>
/// Integration tests for StatusRepository using a real MongoDB container.
/// </summary>
[Collection("StatusIntegration")]
[ExcludeFromCodeCoverage]
public class StatusRepositoryTests
{
	private readonly IStatusRepository _repository;

	public StatusRepositoryTests(MongoDbFixture fixture)
	{
		fixture.ThrowIfUnavailable();
		_repository = new StatusRepository(fixture.ConnectionString, $"T{Guid.NewGuid():N}");
	}

	private static StatusDto CreateTestStatus(string name = "Test Status", string description = "Test Description") =>
		new(
			ObjectId.GenerateNewId(),
			name,
			description,
			DateTime.UtcNow,
			null,
			false,
			UserDto.Empty);

	[Fact]
	public async Task CreateAsync_WithValidStatus_ReturnsOkResult()
	{
		// Arrange
		var status = CreateTestStatus();

		// Act
		var result = await _repository.CreateAsync(status, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
	}

	[Fact]
	public async Task CreateAsync_WithValidStatus_ReturnsCreatedStatus()
	{
		// Arrange
		var status = CreateTestStatus("New Status", "New Description");

		// Act
		var result = await _repository.CreateAsync(status, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
		result.Value?.StatusName.Should().Be("New Status");
	}

	[Fact]
	public async Task GetByIdAsync_WithExistingId_ReturnsStatus()
	{
		// Arrange
		var status = CreateTestStatus();
		var created = await _repository.CreateAsync(status, TestContext.Current.CancellationToken);

		// Act
		var result = await _repository.GetByIdAsync(created.Value!.Id, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
		result.Value?.StatusName.Should().Be(status.StatusName);
	}

	[Fact]
	public async Task GetByIdAsync_WithNonExistentId_ReturnsFailureResult()
	{
		// Arrange
		var nonExistentId = ObjectId.GenerateNewId();

		// Act
		var result = await _repository.GetByIdAsync(nonExistentId, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.NotFound);
	}

	[Fact]
	public async Task GetAllAsync_WithStatuses_ReturnsAllStatuses()
	{
		// Arrange
		await _repository.CreateAsync(CreateTestStatus("Status 1", "Description 1"), TestContext.Current.CancellationToken);
		await _repository.CreateAsync(CreateTestStatus("Status 2", "Description 2"), TestContext.Current.CancellationToken);
		await _repository.CreateAsync(CreateTestStatus("Status 3", "Description 3"), TestContext.Current.CancellationToken);

		// Act
		var result = await _repository.GetAllAsync(TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().HaveCount(3);
	}

	[Fact]
	public async Task GetAllAsync_WithEmptyDatabase_ReturnsEmptyList()
	{
		// Act
		var result = await _repository.GetAllAsync(TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().BeEmpty();
	}

	[Fact]
	public async Task GetAllAsync_WithArchivedStatuses_IncludesThemInResult()
	{
		// Arrange - Create 5 statuses, archive 2
		var created = new List<StatusDto>();
		for (var i = 1; i <= 5; i++)
			created.Add((await _repository.CreateAsync(CreateTestStatus($"Status {i}", $"Description {i}"), TestContext.Current.CancellationToken)).Value!);

		await _repository.ArchiveAsync(created[0].Id, TestContext.Current.CancellationToken);
		await _repository.ArchiveAsync(created[1].Id, TestContext.Current.CancellationToken);

		// Act
		var result = await _repository.GetAllAsync(TestContext.Current.CancellationToken);

		// Assert
		result.Value.Should().HaveCount(5);
	}

	[Fact]
	public async Task ArchiveAsync_WithExistingId_ReturnsOkResult()
	{
		// Arrange
		var status = CreateTestStatus();
		var created = await _repository.CreateAsync(status, TestContext.Current.CancellationToken);

		// Act
		var result = await _repository.ArchiveAsync(created.Value!.Id, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
	}

	[Fact]
	public async Task ArchiveAsync_WithExistingId_SetsArchivedTrue()
	{
		// Arrange
		var status = CreateTestStatus();
		var created = await _repository.CreateAsync(status, TestContext.Current.CancellationToken);

		// Act
		await _repository.ArchiveAsync(created.Value!.Id, TestContext.Current.CancellationToken);
		var getResult = await _repository.GetByIdAsync(created.Value.Id, TestContext.Current.CancellationToken);

		// Assert
		getResult.Value?.Archived.Should().BeTrue();
	}

	[Fact]
	public async Task ArchiveAsync_WithNonExistentId_ReturnsFailureResult()
	{
		// Arrange
		var nonExistentId = ObjectId.GenerateNewId();

		// Act
		var result = await _repository.ArchiveAsync(nonExistentId, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.NotFound);
	}

	[Fact]
	public async Task UpdateAsync_WithExistingStatus_ReturnsOkResult()
	{
		// Arrange
		var status = CreateTestStatus();
		var created = await _repository.CreateAsync(status, TestContext.Current.CancellationToken);
		var updated = created.Value! with { StatusName = "Updated Name" };

		// Act
		var result = await _repository.UpdateAsync(updated, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
	}

	[Fact]
	public async Task UpdateAsync_WithExistingStatus_PersistsChanges()
	{
		// Arrange
		var status = CreateTestStatus();
		var created = await _repository.CreateAsync(status, TestContext.Current.CancellationToken);
		var updated = created.Value! with { StatusName = "Updated Name" };

		// Act
		await _repository.UpdateAsync(updated, TestContext.Current.CancellationToken);
		var getResult = await _repository.GetByIdAsync(created.Value.Id, TestContext.Current.CancellationToken);

		// Assert
		getResult.Value?.StatusName.Should().Be("Updated Name");
	}

	[Fact]
	public async Task UpdateAsync_WithNonExistentStatus_ReturnsFailureResult()
	{
		// Arrange
		var nonExistentStatus = CreateTestStatus() with { Id = ObjectId.GenerateNewId() };

		// Act
		var result = await _repository.UpdateAsync(nonExistentStatus, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.NotFound);
	}
}
