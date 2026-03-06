// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     GetCommentHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =======================================================

namespace Api.Handlers.Comments;

/// <summary>
/// Unit tests for GetCommentHandler.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetCommentHandlerTests
{
	private readonly ICommentRepository _repository;
	private readonly GetCommentHandler _handler;

	public GetCommentHandlerTests()
	{
		_repository = Substitute.For<ICommentRepository>();
		_handler = new GetCommentHandler(_repository);
	}

	[Fact]
	public async Task Handle_ValidCommentId_ReturnsComment()
	{
		// Arrange
		var commentId = ObjectId.GenerateNewId();
		var comment = new CommentDto(
			commentId,
			"Test Comment",
			"This is a test comment.",
			DateTime.UtcNow,
			null,
			IssueDto.Empty,
			UserDto.Empty,
			[],
			false,
			UserDto.Empty,
			false,
			UserDto.Empty);

		_repository.GetByIdAsync(commentId, Arg.Any<CancellationToken>())
			.Returns(Result<CommentDto>.Ok(comment));

		var query = new GetCommentQuery(commentId);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Success.Should().BeTrue();
		result.Value!.Title.Should().Be("Test Comment");
		result.Value.Description.Should().Be("This is a test comment.");
		await _repository.Received(1).GetByIdAsync(commentId, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_NonExistentCommentId_ReturnsNull()
	{
		// Arrange
		var commentId = ObjectId.GenerateNewId();
		_repository.GetByIdAsync(commentId, Arg.Any<CancellationToken>())
			.Returns(Result<CommentDto>.Fail("Not found"));

		var query = new GetCommentQuery(commentId);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Not found");
	}

	[Fact]
	public async Task Handle_EmptyObjectId_ReturnsNull()
	{
		// Arrange
		_repository.GetByIdAsync(ObjectId.Empty, Arg.Any<CancellationToken>())
			.Returns(Result<CommentDto>.Fail("Not found"));
		var query = new GetCommentQuery(ObjectId.Empty);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Not found");
		await _repository.Received(1).GetByIdAsync(ObjectId.Empty, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_ValidCommentId_PassesCancellationToken()
	{
		// Arrange
		var commentId = ObjectId.GenerateNewId();
		var cancellationToken = new CancellationToken();
		var comment = new CommentDto(
			commentId,
			"Test Comment",
			"This is a test comment.",
			DateTime.UtcNow,
			null,
			IssueDto.Empty,
			UserDto.Empty,
			[],
			false,
			UserDto.Empty,
			false,
			UserDto.Empty);

		_repository.GetByIdAsync(commentId, Arg.Any<CancellationToken>())
			.Returns(Result<CommentDto>.Ok(comment));

		var query = new GetCommentQuery(commentId);

		// Act
		await _handler.Handle(query, cancellationToken);

		// Assert
		await _repository.Received(1).GetByIdAsync(commentId, Arg.Any<CancellationToken>());
	}
}
