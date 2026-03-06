// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateCommentHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Integration
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for UpdateCommentHandler with a real MongoDB database.
/// </summary>
[Collection("Integration")]
[ExcludeFromCodeCoverage]
public class UpdateCommentHandlerIntegrationTests : IAsyncLifetime
{
	private const string MongodbImage = "mongo:latest";
	private const string TestDatabase = "IssueManagerTestDb";
	private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder(MongodbImage)
		.Build();

	private ICommentRepository _repository = null!;
	private UpdateCommentHandler _handler = null!;

	/// <summary>
	/// Initializes the test container and repository.
	/// </summary>
	public async ValueTask InitializeAsync()
	{
		await _mongoContainer.StartAsync();
		var connectionString = _mongoContainer.GetConnectionString();
		_repository = new CommentRepository(connectionString, TestDatabase);
		_handler = new UpdateCommentHandler(_repository, new UpdateCommentValidator());
	}

	/// <summary>
	/// Disposes the test container.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		await _mongoContainer.StopAsync();
		await _mongoContainer.DisposeAsync();
	}

	private static CommentDto CreateTestCommentDto(string title, string description = "Test description") =>
		new(ObjectId.GenerateNewId(), title, description, DateTime.UtcNow, null, IssueDto.Empty,
			UserDto.Empty, [], false, UserDto.Empty, false, UserDto.Empty);

	[Fact]
	public async Task Handle_ExistingComment_UpdatesSuccessfully()
	{
		// Arrange - Create a comment
		var comment = CreateTestCommentDto("Original Title", "Original Description");
		var created = await _repository.CreateAsync(comment, TestContext.Current.CancellationToken);

		var command = new UpdateCommentCommand
		{
			Id = created.Value!.Id,
			Title = "Updated Title",
			CommentText = "Updated comment text"
		};

		// Act
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeTrue();
		result.Value!.Title.Should().Be("Updated Title");
		result.Value!.Id.Should().Be(created.Value.Id);
	}

	[Fact]
	public async Task Handle_NonExistentComment_ThrowsNotFoundException()
	{
		// Arrange
		var nonExistentId = ObjectId.GenerateNewId();
		var command = new UpdateCommentCommand
		{
			Id = nonExistentId,
			Title = "Updated Title",
			CommentText = "Non-existent comment text"
		};

		// Act
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
	}

	[Fact]
	public async Task Handle_InvalidCommand_ThrowsValidationException()
	{
		// Arrange - Empty title is invalid
		var comment = CreateTestCommentDto("Test Comment", "Test Description");
		var created = await _repository.CreateAsync(comment, TestContext.Current.CancellationToken);

		var command = new UpdateCommentCommand
		{
			Id = created.Value!.Id,
			Title = string.Empty
		};

		// Act
		var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

		// Assert
		result.Success.Should().BeFalse();
	}
}
