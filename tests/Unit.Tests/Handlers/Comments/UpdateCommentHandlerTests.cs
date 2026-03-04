// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateCommentHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

namespace Unit.Handlers.Comments;

/// <summary>
/// Unit tests for UpdateCommentHandler.
/// </summary>
[ExcludeFromCodeCoverage]
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
		var existingCommentDto = CommentDto.Empty with { Id = commentId, Title = "Old Title", Description = "Old comment text." };
		var updatedCommentDto = CommentDto.Empty with { Id = commentId, Title = "Updated Title", Description = "Updated comment text." };

		var command = new UpdateCommentCommand
		{
			Id = commentId,
			Title = "Updated Title",
			CommentText = "Updated comment text."
		};

		_repository.GetByIdAsync(commentId, Arg.Any<CancellationToken>())
			.Returns(Result<CommentDto>.Ok(existingCommentDto));

		_repository.UpdateAsync(Arg.Any<CommentDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<CommentDto>.Ok(updatedCommentDto));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Title.Should().Be("Updated Title");
		await _repository.Received(1).UpdateAsync(Arg.Is<CommentDto>(c =>
			c.Title == command.Title), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_EmptyId_ThrowsValidationException()
	{
		// Arrange
		var command = new UpdateCommentCommand
		{
			Id = ObjectId.Empty,
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
			Id = ObjectId.GenerateNewId(),
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
			Id = ObjectId.GenerateNewId(),
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
			Id = commentId,
			Title = "Updated Title",
			CommentText = "Updated comment text."
		};

		_repository.GetByIdAsync(commentId, Arg.Any<CancellationToken>())
			.Returns(Result<CommentDto>.Fail("Not found"));

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>()
			.WithMessage($"*{commentId}*");
	}

	[Fact]
	public async Task Handle_RepositoryUpdateFails_ThrowsNotFoundException()
	{
		// Arrange
		var commentId = ObjectId.GenerateNewId();
		var existingCommentDto = CommentDto.Empty with { Id = commentId, Title = "Old Title", Description = "Old comment text." };

		var command = new UpdateCommentCommand
		{
			Id = commentId,
			Title = "Updated Title",
			CommentText = "Updated comment text."
		};

		_repository.GetByIdAsync(commentId, Arg.Any<CancellationToken>())
			.Returns(Result<CommentDto>.Ok(existingCommentDto));

		_repository.UpdateAsync(Arg.Any<CommentDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<CommentDto>.Fail("Update failed"));

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>()
			.WithMessage("*could not be updated*");
	}
}
