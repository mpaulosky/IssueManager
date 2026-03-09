// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateIssueHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =======================================================

using Api.Data.Interfaces;

namespace Api.Handlers.Issues;

/// <summary>
/// Unit tests for CreateIssueHandler.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreateIssueHandlerTests
{
	private readonly IIssueRepository _repository;
	private readonly CreateIssueValidator _validator;
	private readonly ICurrentUserService _currentUserService;
	private readonly CreateIssueHandler _handler;

	public CreateIssueHandlerTests()
	{
		_repository = Substitute.For<IIssueRepository>();
		_validator = new CreateIssueValidator();
		_currentUserService = Substitute.For<ICurrentUserService>();
		_currentUserService.UserId.Returns("test-user-id");
		_currentUserService.Name.Returns("Test User");
		_currentUserService.Email.Returns("test@example.com");
		_currentUserService.IsAuthenticated.Returns(true);
		_handler = new CreateIssueHandler(_repository, _validator, _currentUserService);
	}

	[Fact]
	public async Task Handle_ValidCommand_ReturnsCreatedIssue()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = "New Issue",
			Description = "New issue description"
		};

		var createdIssue = new IssueDto(
			ObjectId.GenerateNewId(),
			command.Title,
			command.Description ?? string.Empty,
			DateTime.UtcNow,
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			false,
			UserDto.Empty,
			false,
			false);

		_repository.CreateAsync(Arg.Any<IssueDto>(), Arg.Any<CancellationToken>())
			.Returns(Result.Ok(createdIssue));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value!.Title.Should().Be(command.Title);
		result.Value!.Description.Should().Be(command.Description);
		await _repository.Received(1).CreateAsync(Arg.Any<IssueDto>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_EmptyTitle_ReturnsValidationFailure()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = "",
			Description = "Description"
		};

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
	}

	[Fact]
	public async Task Handle_TitleTooLong_ReturnsValidationFailure()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = new string('A', 201),
			Description = "Description"
		};

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
	}

	[Fact]
	public async Task Handle_DescriptionTooLong_ReturnsValidationFailure()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = "Valid Title",
			Description = new string('X', 5001)
		};

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
	}

	[Fact]
	public async Task Handle_NullDescription_UsesEmptyString()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = "Valid Title",
			Description = null
		};

		var createdIssue = new IssueDto(
			ObjectId.GenerateNewId(),
			command.Title,
			string.Empty,
			DateTime.UtcNow,
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			false,
			UserDto.Empty,
			false,
			false);

		_repository.CreateAsync(Arg.Any<IssueDto>(), Arg.Any<CancellationToken>())
			.Returns(Result.Ok(createdIssue));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeTrue();
		result.Value!.Description.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_ValidCommand_PassesCancellationToken()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = "Test Issue",
			Description = "Test description"
		};

		var cancellationToken = new CancellationToken();
		var createdIssue = new IssueDto(
			ObjectId.GenerateNewId(),
			command.Title,
			command.Description ?? string.Empty,
			DateTime.UtcNow,
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			false,
			UserDto.Empty,
			false,
			false);

		_repository.CreateAsync(Arg.Any<IssueDto>(), cancellationToken)
			.Returns(Result.Ok(createdIssue));

		// Act
		await _handler.Handle(command, cancellationToken);

		// Assert
		await _repository.Received(1).CreateAsync(Arg.Any<IssueDto>(), cancellationToken);
	}
}
