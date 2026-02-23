// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateCommentHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

using Api.Data;
using Api.Handlers;
using MongoDB.Bson;
using Shared.Abstractions;
using Shared.Exceptions;
using Shared.Models;
using Shared.Validators;

namespace Tests.Unit.Handlers.Comments;

/// <summary>
/// Unit tests for UpdateCommentHandler.
/// </summary>
public class UpdateCommentHandlerTests
{
	private readonly ICommentRepository _repository;
	private readonly UpdateCommentValidator _validator;
	private readonly UpdateCommentHandler _handler;

	public UpdateCommentHandlerTests()
	{
		_repository = Substitute.For<ICommentRepository>();
		_validator = new UpdateCommentValidator();
		_handler = new UpdateCommentHandler(_repository, _validator);
	}

	[Fact]
	public async Task Handle_ValidCommand_ReturnsUpdatedComment()
	{
		// Arrange
		var commentId = ObjectId.GenerateNewId();
		var existingComment = new Comment
		{
			Id = commentId,
			Title = "Old Title",
			Description = "Old comment text."
		};

		var command = new UpdateCommentCommand
		{
			Id = commentId.ToString(),
			Title = "Updated Title",
			CommentText = "Updated comment text."
		};

		_repository.GetAsync(commentId)
			.Returns(Result<Comment>.Ok(existingComment));

		_repository.UpdateAsync(commentId, Arg.Any<Comment>())
			.Returns(Result.Ok());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Title.Should().Be("Updated Title");
		await _repository.Received(1).UpdateAsync(commentId, Arg.Is<Comment>(c =>
			c.Title == command.Title));
	}

	[Fact]
	public async Task Handle_EmptyId_ThrowsValidationException()
	{
		// Arrange
		var command = new UpdateCommentCommand
		{
			Id = "",
			Title = "Updated Title",
			CommentText = "Updated comment text."
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Comment ID*required*");
	}

	[Fact]
	public async Task Handle_EmptyCommentText_ThrowsValidationException()
	{
		// Arrange
		var command = new UpdateCommentCommand
		{
			Id = ObjectId.GenerateNewId().ToString(),
			Title = "Updated Title",
			CommentText = ""
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Comment text*required*");
	}

	[Fact]
	public async Task Handle_CommentTextTooLong_ThrowsValidationException()
	{
		// Arrange
		var command = new UpdateCommentCommand
		{
			Id = ObjectId.GenerateNewId().ToString(),
			Title = "Updated Title",
			CommentText = new string('A', 5001)
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Comment text*5000 characters*");
	}

	[Fact]
	public async Task Handle_NonExistentComment_ThrowsNotFoundException()
	{
		// Arrange
		var commentId = ObjectId.GenerateNewId();
		var command = new UpdateCommentCommand
		{
			Id = commentId.ToString(),
			Title = "Updated Title",
			CommentText = "Updated comment text."
		};

		_repository.GetAsync(commentId)
			.Returns(Result<Comment>.Fail("Not found"));

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>()
			.WithMessage($"*{commentId}*");
	}

	[Fact]
	public async Task Handle_InvalidObjectId_ThrowsNotFoundException()
	{
		// Arrange
		var command = new UpdateCommentCommand
		{
			Id = "invalid-objectid",
			Title = "Updated Title",
			CommentText = "Updated comment text."
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>();
	}

	[Fact]
	public async Task Handle_RepositoryUpdateFails_ThrowsNotFoundException()
	{
		// Arrange
		var commentId = ObjectId.GenerateNewId();
		var existingComment = new Comment
		{
			Id = commentId,
			Title = "Old Title",
			Description = "Old comment text."
		};

		var command = new UpdateCommentCommand
		{
			Id = commentId.ToString(),
			Title = "Updated Title",
			CommentText = "Updated comment text."
		};

		_repository.GetAsync(commentId)
			.Returns(Result<Comment>.Ok(existingComment));

		_repository.UpdateAsync(commentId, Arg.Any<Comment>())
			.Returns(Result.Fail("Update failed"));

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>()
			.WithMessage("*could not be updated*");
	}
}
