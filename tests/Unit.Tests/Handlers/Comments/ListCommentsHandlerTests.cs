// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ListCommentsHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

namespace Unit.Handlers.Comments;

/// <summary>
/// Unit tests for ListCommentsHandler.
/// </summary>
[ExcludeFromCodeCoverage]
public class ListCommentsHandlerTests
{
	private readonly ICommentRepository _repository;
	private readonly ListCommentsHandler _handler;

	public ListCommentsHandlerTests()
	{
		_repository = Substitute.For<ICommentRepository>();
		_handler = new ListCommentsHandler(_repository);
	}

	[Fact]
	public async Task Handle_ReturnsAllComments()
	{
		// Arrange
		IReadOnlyList<CommentDto> comments = new List<CommentDto>
		{
			new(ObjectId.GenerateNewId(), "First Comment", "First comment text.", DateTime.UtcNow, null, IssueDto.Empty, UserDto.Empty, [], false, UserDto.Empty, false, UserDto.Empty),
			new(ObjectId.GenerateNewId(), "Second Comment", "Second comment text.", DateTime.UtcNow, null, IssueDto.Empty, UserDto.Empty, [], false, UserDto.Empty, false, UserDto.Empty),
			new(ObjectId.GenerateNewId(), "Third Comment", "Third comment text.", DateTime.UtcNow, null, IssueDto.Empty, UserDto.Empty, [], false, UserDto.Empty, false, UserDto.Empty)
		};

		_repository.GetAllAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Result<IReadOnlyList<CommentDto>>.Ok(comments));

		// Act
		var result = await _handler.Handle(cancellationToken: CancellationToken.None);

		// Assert
		result.Should().HaveCount(3);
		result.Should().Contain(c => c.Title == "First Comment");
		result.Should().Contain(c => c.Title == "Second Comment");
		result.Should().Contain(c => c.Title == "Third Comment");
	}

	[Fact]
	public async Task Handle_NoComments_ReturnsEmptyList()
	{
		// Arrange
		_repository.GetAllAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Result<IReadOnlyList<CommentDto>>.Ok((IReadOnlyList<CommentDto>)new List<CommentDto>()));

		// Act
		var result = await _handler.Handle(cancellationToken: CancellationToken.None);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_RepositoryFails_ReturnsEmptyList()
	{
		// Arrange
		_repository.GetAllAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Result<IReadOnlyList<CommentDto>>.Fail("Database error"));

		// Act
		var result = await _handler.Handle(cancellationToken: CancellationToken.None);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_RepositoryReturnsNull_ReturnsEmptyList()
	{
		// Arrange
		_repository.GetAllAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Result<IReadOnlyList<CommentDto>>.Ok((IReadOnlyList<CommentDto>)null!));

		// Act
		var result = await _handler.Handle(cancellationToken: CancellationToken.None);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_PassesCancellationToken()
	{
		// Arrange
		var cancellationToken = new CancellationToken();
		IReadOnlyList<CommentDto> comments = new List<CommentDto>
		{
			new(ObjectId.GenerateNewId(), "Test Comment", string.Empty, DateTime.UtcNow, null, IssueDto.Empty, UserDto.Empty, [], false, UserDto.Empty, false, UserDto.Empty)
		};

		_repository.GetAllAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
			.Returns(Result<IReadOnlyList<CommentDto>>.Ok(comments));

		// Act
		await _handler.Handle(cancellationToken: cancellationToken);

		// Assert
		await _repository.Received(1).GetAllAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
	}
}
