// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateIssueHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Integration
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for UpdateIssueHandler with a real MongoDB database.
/// </summary>
[Collection("IssueIntegration")]
[ExcludeFromCodeCoverage]
public class UpdateIssueHandlerIntegrationTests
{
	private readonly IIssueRepository _repository;
	private readonly UpdateIssueHandler _handler;

	public UpdateIssueHandlerIntegrationTests(MongoDbFixture fixture)
	{
		fixture.ThrowIfUnavailable();
		_repository = new IssueRepository(fixture.ConnectionString, $"T{Guid.NewGuid():N}");
		_handler = new UpdateIssueHandler(_repository, new UpdateIssueValidator());
	}

	private static IssueDto CreateTestIssueDto(string title, string description, bool archived = false) =>
	new(ObjectId.GenerateNewId(), title, description, DateTime.UtcNow, null, UserDto.Empty, CategoryDto.Empty, StatusDto.Empty, archived, UserDto.Empty, false, false);

	[Fact]
	public async Task Handle_ValidUpdate_UpdatesIssueInDatabase()
	{
		// Arrange - Create an issue first
		var originalIssue = CreateTestIssueDto("Original Title", "Original Description");
		var created = await _repository.CreateAsync(originalIssue, TestContext.Current.CancellationToken);

		var command = new UpdateIssueCommand
		{
			Id = created.Value!.Id,
			Title = "Updated Title",
			Description = "Updated Description"
		};

		// Act
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
		result.Value!.Id.Should().Be(created.Value!.Id);
		result.Value!.Title.Should().Be("Updated Title");
		result.Value!.Description.Should().Be("Updated Description");

		// Verify in database
		var dbIssueResult = await _repository.GetByIdAsync(created.Value.Id, TestContext.Current.CancellationToken);
		dbIssueResult.Should().NotBeNull();
		var dbIssue = dbIssueResult.Value;
		dbIssue?.Title.Should().Be("Updated Title");
		dbIssue?.Description.Should().Be("Updated Description");
	}

	[Fact]
	public async Task Handle_AtomicUpdate_TitleAndDescriptionBothUpdate()
	{
		// Arrange
		var originalIssue = CreateTestIssueDto("Original Title", "Original Description");
		var created = await _repository.CreateAsync(originalIssue, TestContext.Current.CancellationToken);

		var command = new UpdateIssueCommand
		{
			Id = created.Value!.Id,
			Title = "New Title",
			Description = "New Description"
		};

		// Act
		await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert - Both fields should be updated atomically
		var dbIssueResult = await _repository.GetByIdAsync(created.Value!.Id, TestContext.Current.CancellationToken);
		dbIssueResult.Should().NotBeNull();
		var dbIssue = dbIssueResult.Value;
		dbIssue?.Title.Should().Be("New Title");
		dbIssue?.Description.Should().Be("New Description");
	}

	[Fact]
	public async Task Handle_NonExistentIssue_ThrowsNotFoundException()
	{
		// Arrange
		var nonExistentId = ObjectId.GenerateNewId();
		var command = new UpdateIssueCommand
		{
			Id = nonExistentId,
			Title = "Title",
			Description = "Description"
		};

		// Act
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
	}

	[Fact]
	public async Task Handle_ConcurrentUpdates_LastWriteWins()
	{
		// Arrange - Create an issue
		var issue = CreateTestIssueDto("Original Title", "Original Description");
		var created = await _repository.CreateAsync(issue, TestContext.Current.CancellationToken);

		var command1 = new UpdateIssueCommand
		{
			Id = created.Value!.Id,
			Title = "First Update",
			Description = "First Description"
		};

		var command2 = new UpdateIssueCommand
		{
			Id = created.Value!.Id,
			Title = "Second Update",
			Description = "Second Description"
		};

		// Act - Simulate sequential updates
		await _handler.Handle(command1, TestContext.Current.CancellationToken);
		await Task.Delay(100, TestContext.Current.CancellationToken); // Small delay to ensure different ordering
		await _handler.Handle(command2, TestContext.Current.CancellationToken);

		// Assert - Last write wins
		var dbIssueResult = await _repository.GetByIdAsync(created.Value!.Id, TestContext.Current.CancellationToken);
		dbIssueResult.Should().NotBeNull();
		var dbIssue = dbIssueResult.Value;
		dbIssue?.Title.Should().Be("Second Update");
		dbIssue?.Description.Should().Be("Second Description");
	}

	[Fact]
	public async Task Handle_ArchivedIssue_ThrowsConflictException()
	{
		// Arrange - Create and archive an issue
		var archivedIssue = CreateTestIssueDto("Archived Issue", "This is archived", archived: true);
		var created = await _repository.CreateAsync(archivedIssue, TestContext.Current.CancellationToken);

		var command = new UpdateIssueCommand
		{
			Id = created.Value!.Id,
			Title = "Attempt Update",
			Description = "Should fail"
		};

		// Act
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();

		// Verify the issue wasn't updated
		var dbIssueResult = await _repository.GetByIdAsync(created.Value!.Id, TestContext.Current.CancellationToken);
		var dbIssue = dbIssueResult.Value;
		dbIssue?.Title.Should().Be("Archived Issue");
	}
}
