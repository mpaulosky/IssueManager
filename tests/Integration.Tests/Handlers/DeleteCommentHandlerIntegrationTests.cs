// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteCommentHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Integration.Tests
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for DeleteCommentHandler (soft-delete via Archived) with real MongoDB database.
/// </summary>
[Collection("Integration")]
[ExcludeFromCodeCoverage]
public class DeleteCommentHandlerIntegrationTests : IAsyncLifetime
{
	private const string MongodbImage = "mongo:latest";
	private const string TestDatabase = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder(MongodbImage)
		.Build();

	private ICommentRepository _repository = null!;
	private DeleteCommentHandler _handler = null!;

	/// <summary>
	/// Initializes the test container and repository.
	/// </summary>
	public async ValueTask InitializeAsync()
	{
		await _mongoContainer.StartAsync();
		var connectionString = _mongoContainer.GetConnectionString();
		_repository = new CommentRepository(connectionString, TestDatabase);
		_handler = new DeleteCommentHandler(_repository, new DeleteCommentValidator());
	}

	/// <summary>
	/// Disposes the test container.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		await _mongoContainer.StopAsync();
		await _mongoContainer.DisposeAsync();
	}

	private static CommentDto CreateTestCommentDto(string title, string description = "Test description", bool archived = false) =>
		new(ObjectId.GenerateNewId(), title, description, DateTime.UtcNow, null, IssueDto.Empty,
			UserDto.Empty, [], archived, UserDto.Empty, false, UserDto.Empty);

	[Fact]
	public async Task Handle_ValidComment_SetsArchivedInDatabase()
	{
		// Arrange - Create a comment
		var comment = CreateTestCommentDto("Comment to Delete", "This will be archived");
		var created = await _repository.CreateAsync(comment, TestContext.Current.CancellationToken);

		var command = new DeleteCommentCommand { Id = created.Value.Id.ToString() };

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
	public async Task Handle_NonExistentComment_ThrowsNotFoundException()
	{
		// Arrange
		var nonExistentId = ObjectId.GenerateNewId().ToString();
		var command = new DeleteCommentCommand { Id = nonExistentId };

		// Act
		Func<Task> act = async () => await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>();
	}

	[Fact]
	public async Task Handle_AlreadyArchivedComment_IsIdempotent()
	{
		// Arrange - Create an already archived comment
		var archivedComment = CreateTestCommentDto("Already Archived", "Already archived", archived: true);
		var created = await _repository.CreateAsync(archivedComment, TestContext.Current.CancellationToken);

		var command = new DeleteCommentCommand { Id = created.Value.Id.ToString() };

		// Act - Delete already archived comment (should be idempotent)
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert - Should still return true
		result.Should().BeTrue();

		var dbCommentResult = await _repository.GetByIdAsync(created.Value.Id, TestContext.Current.CancellationToken);
		dbCommentResult.Should().NotBeNull();
		dbCommentResult.Value.Archived.Should().BeTrue();
	}

	[Fact]
	public async Task Handle_CommentNotDeleted_RecordStillExists()
	{
		// Arrange - Create a comment
		var comment = CreateTestCommentDto("Comment to Archive", "Should still exist in DB");
		var created = await _repository.CreateAsync(comment, TestContext.Current.CancellationToken);

		var command = new DeleteCommentCommand { Id = created.Value.Id.ToString() };

		// Act - Soft delete
		await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert - Record should still exist (soft delete)
		var dbComment = await _repository.GetByIdAsync(created.Value.Id, TestContext.Current.CancellationToken);
		dbComment.Should().NotBeNull();
		dbComment.Value.Id.Should().Be(created.Value.Id);
		dbComment.Value.Archived.Should().BeTrue();
	}
}
