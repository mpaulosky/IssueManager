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
[Collection("CommentIntegration")]
[ExcludeFromCodeCoverage]
public class UpdateCommentHandlerIntegrationTests
{
	private readonly ICommentRepository _repository;
	private readonly UpdateCommentHandler _handler;

	public UpdateCommentHandlerIntegrationTests(MongoDbFixture fixture)
	{
		fixture.ThrowIfUnavailable();
		_repository = new CommentRepository(fixture.ConnectionString, $"T{Guid.NewGuid():N}");
		_handler = new UpdateCommentHandler(_repository, new UpdateCommentValidator());
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
