// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteCommentHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =======================================================

namespace Unit.Handlers.Comments;

/// <summary>
/// Unit tests for DeleteCommentHandler (soft-delete via Archived).
/// </summary>
[ExcludeFromCodeCoverage]
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
		var comment = new CommentDto(
			commentId,
			"Test Comment",
			string.Empty,
			DateTime.UtcNow,
			null,
			IssueDto.Empty,
			UserDto.Empty,
			[],
			false,
			UserDto.Empty,
			false,
			UserDto.Empty);

		var command = new DeleteCommentCommand { Id = commentId };

		_repository.GetByIdAsync(commentId, Arg.Any<CancellationToken>())
			.Returns(Result<CommentDto>.Ok(comment));

		_repository.ArchiveAsync(commentId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().BeTrue();
		await _repository.Received(1).GetByIdAsync(commentId, Arg.Any<CancellationToken>());
		await _repository.Received(1).ArchiveAsync(commentId, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_NonExistentComment_ReturnsNotFoundResult()
	{
		// Arrange
		var commentId = ObjectId.GenerateNewId();
		var command = new DeleteCommentCommand { Id = commentId };

		_repository.GetByIdAsync(commentId, Arg.Any<CancellationToken>())
			.Returns(Result<CommentDto>.Fail("Not found"));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.NotFound);
	}

	[Fact]
	public async Task Handle_AlreadyArchivedComment_IsIdempotent()
	{
		// Arrange
		var commentId = ObjectId.GenerateNewId();
		var archivedComment = new CommentDto(
			commentId,
			"Archived Comment",
			string.Empty,
			DateTime.UtcNow,
			null,
			IssueDto.Empty,
			UserDto.Empty,
			[],
			true,
			UserDto.Empty,
			false,
			UserDto.Empty);

		var command = new DeleteCommentCommand { Id = commentId };

		_repository.GetByIdAsync(commentId, Arg.Any<CancellationToken>())
			.Returns(Result<CommentDto>.Ok(archivedComment));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().BeTrue();
		await _repository.Received(1).GetByIdAsync(commentId, Arg.Any<CancellationToken>());
		await _repository.DidNotReceive().ArchiveAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_EmptyId_ReturnsValidationFailure()
	{
		// Arrange
		var command = new DeleteCommentCommand { Id = ObjectId.Empty };

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
	}

	[Fact]
	public async Task Handle_ValidComment_PassesCancellationToken()
	{
		// Arrange
		var commentId = ObjectId.GenerateNewId();
		var cancellationToken = new CancellationToken();
		var comment = new CommentDto(
			commentId,
			"Test Comment",
			string.Empty,
			DateTime.UtcNow,
			null,
			IssueDto.Empty,
			UserDto.Empty,
			[],
			false,
			UserDto.Empty,
			false,
			UserDto.Empty);

		var command = new DeleteCommentCommand { Id = commentId };

		_repository.GetByIdAsync(commentId, Arg.Any<CancellationToken>())
			.Returns(Result<CommentDto>.Ok(comment));

		_repository.ArchiveAsync(commentId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok());

		// Act
		await _handler.Handle(command, cancellationToken);

		// Assert
		await _repository.Received(1).GetByIdAsync(commentId, Arg.Any<CancellationToken>());
		await _repository.Received(1).ArchiveAsync(commentId, Arg.Any<CancellationToken>());
	}
}
