// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateCommentHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

namespace Unit.Handlers.Comments;

/// <summary>
/// Unit tests for CreateCommentHandler.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreateCommentHandlerTests
{
	private readonly ICommentRepository _repository;
	private readonly CreateCommentValidator _validator;
	private readonly ICurrentUserService _currentUserService;
	private readonly CreateCommentHandler _handler;

	public CreateCommentHandlerTests()
	{
		_repository = Substitute.For<ICommentRepository>();
		_validator = new CreateCommentValidator();
		_currentUserService = Substitute.For<ICurrentUserService>();
		_currentUserService.UserId.Returns("test-user-id");
		_currentUserService.Name.Returns("Test User");
		_currentUserService.Email.Returns("test@example.com");
		_currentUserService.IsAuthenticated.Returns(true);
		_handler = new CreateCommentHandler(_repository, _validator, _currentUserService);
	}

	[Fact]
	public async Task Handle_ValidCommand_ReturnsCreatedComment()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId().ToString();
		var command = new CreateCommentCommand
		{
			Title = "Test Comment",
			CommentText = "This is a valid comment text.",
			IssueId = issueId
		};

		_repository.CreateAsync(Arg.Any<CommentDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<CommentDto>.Ok(CommentDto.Empty with { Title = command.Title, Description = command.CommentText }));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Title.Should().Be(command.Title);
		result.Description.Should().Be(command.CommentText);
		await _repository.Received(1).CreateAsync(Arg.Is<CommentDto>(c =>
			c.Title == command.Title &&
			c.Description == command.CommentText), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_EmptyCommentText_ThrowsValidationException()
	{
		// Arrange
		var command = new CreateCommentCommand
		{
			Title = "Test Comment",
			CommentText = "",
			IssueId = ObjectId.GenerateNewId().ToString()
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
		var command = new CreateCommentCommand
		{
			Title = "Test Comment",
			CommentText = new string('A', 5001),
			IssueId = ObjectId.GenerateNewId().ToString()
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Comment text*5000 characters*");
	}

	[Fact]
	public async Task Handle_EmptyIssueId_ThrowsValidationException()
	{
		// Arrange
		var command = new CreateCommentCommand
		{
			Title = "Test Comment",
			CommentText = "Valid comment text.",
			IssueId = ""
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Issue ID*required*");
	}

	[Fact]
	public async Task Handle_InvalidObjectIdForIssueId_ThrowsValidationException()
	{
		// Arrange
		var command = new CreateCommentCommand
		{
			Title = "Test Comment",
			CommentText = "Valid comment text.",
			IssueId = "not-a-valid-objectid"
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Issue ID*valid ObjectId*");
	}

	[Fact]
	public async Task Handle_RepositoryFails_ThrowsInvalidOperationException()
	{
		// Arrange
		var command = new CreateCommentCommand
		{
			Title = "Test Comment",
			CommentText = "Valid comment text.",
			IssueId = ObjectId.GenerateNewId().ToString()
		};

		_repository.CreateAsync(Arg.Any<CommentDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<CommentDto>.Fail("Database error"));

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("Database error");
	}

	[Fact]
	public async Task Handle_ValidCommand_PassesCancellationToken()
	{
		// Arrange
		var cancellationToken = new CancellationToken();
		var command = new CreateCommentCommand
		{
			Title = "Test Comment",
			CommentText = "Valid comment text.",
			IssueId = ObjectId.GenerateNewId().ToString()
		};

		_repository.CreateAsync(Arg.Any<CommentDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<CommentDto>.Ok(CommentDto.Empty));

		// Act
		await _handler.Handle(command, cancellationToken);

		// Assert
		await _repository.Received(1).CreateAsync(Arg.Any<CommentDto>(), Arg.Any<CancellationToken>());
	}
}
