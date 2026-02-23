// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteCommentHandlerTests.cs
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
/// Unit tests for DeleteCommentHandler (soft-delete via Archived).
/// </summary>
public class DeleteCommentHandlerTests
{
	private readonly ICommentRepository _repository;
	private readonly DeleteCommentValidator _validator;
	private readonly DeleteCommentHandler _handler;

	public DeleteCommentHandlerTests()
	{
		_repository = Substitute.For<ICommentRepository>();
		_validator = new DeleteCommentValidator();
		_handler = new DeleteCommentHandler(_repository, _validator);
	}

	[Fact]
	public async Task Handle_ValidComment_SetsIsArchivedToTrue()
	{
		// Arrange
		var commentId = ObjectId.GenerateNewId();
		var comment = new Comment
		{
			Id = commentId,
			Title = "Test Comment",
			Archived = false
		};

		var command = new DeleteCommentCommand { Id = commentId.ToString() };

		_repository.GetAsync(commentId)
			.Returns(Result<Comment>.Ok(comment));

		_repository.ArchiveAsync(comment)
			.Returns(Result.Ok());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().BeTrue();
		await _repository.Received(1).GetAsync(commentId);
		await _repository.Received(1).ArchiveAsync(comment);
	}

	[Fact]
	public async Task Handle_NonExistentComment_ThrowsNotFoundException()
	{
		// Arrange
		var commentId = ObjectId.GenerateNewId();
		var command = new DeleteCommentCommand { Id = commentId.ToString() };

		_repository.GetAsync(commentId)
			.Returns(Result<Comment>.Fail("Not found"));

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>()
			.WithMessage($"*{commentId}*");
	}

	[Fact]
	public async Task Handle_AlreadyArchivedComment_IsIdempotent()
	{
		// Arrange
		var commentId = ObjectId.GenerateNewId();
		var archivedComment = new Comment
		{
			Id = commentId,
			Title = "Archived Comment",
			Archived = true
		};

		var command = new DeleteCommentCommand { Id = commentId.ToString() };

		_repository.GetAsync(commentId)
			.Returns(Result<Comment>.Ok(archivedComment));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().BeTrue();
		await _repository.Received(1).GetAsync(commentId);
		await _repository.DidNotReceive().ArchiveAsync(Arg.Any<Comment>());
	}

	[Fact]
	public async Task Handle_EmptyId_ThrowsValidationException()
	{
		// Arrange
		var command = new DeleteCommentCommand { Id = "" };

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Comment ID*required*");
	}

	[Fact]
	public async Task Handle_InvalidObjectId_ThrowsNotFoundException()
	{
		// Arrange
		var command = new DeleteCommentCommand { Id = "invalid-objectid" };

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>();
	}

	[Fact]
	public async Task Handle_ValidComment_PassesCancellationToken()
	{
		// Arrange
		var commentId = ObjectId.GenerateNewId();
		var cancellationToken = new CancellationToken();
		var comment = new Comment
		{
			Id = commentId,
			Title = "Test Comment",
			Archived = false
		};

		var command = new DeleteCommentCommand { Id = commentId.ToString() };

		_repository.GetAsync(commentId)
			.Returns(Result<Comment>.Ok(comment));

		_repository.ArchiveAsync(comment)
			.Returns(Result.Ok());

		// Act
		await _handler.Handle(command, cancellationToken);

		// Assert
		await _repository.Received(1).GetAsync(commentId);
		await _repository.Received(1).ArchiveAsync(comment);
	}
}
