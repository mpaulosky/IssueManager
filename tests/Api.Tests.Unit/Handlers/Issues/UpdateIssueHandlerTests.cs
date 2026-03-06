// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateIssueHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =======================================================
namespace Unit.Handlers.Issues;

/// <summary>
/// Unit tests for UpdateIssueHandler.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateIssueHandlerTests
{
	private readonly IIssueRepository _repository;
	private readonly UpdateIssueValidator _validator;
	private readonly UpdateIssueHandler _handler;

	public UpdateIssueHandlerTests()
	{
		_repository = Substitute.For<IIssueRepository>();
		_validator = new UpdateIssueValidator();
		_handler = new UpdateIssueHandler(_repository, _validator);
	}

	[Fact]
	public async Task Handle_ValidCommand_ReturnsUpdatedIssue()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId();
		var existingIssue = new IssueDto(
			issueId,
			"Original Title",
			"Original Description",
			DateTime.UtcNow.AddDays(-1),
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			false,
			UserDto.Empty,
			false,
			false);

		var command = new UpdateIssueCommand
		{
			Id = issueId,
			Title = "Updated Title",
			Description = "Updated Description"
		};

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok(existingIssue));

		var updatedIssue = existingIssue with
		{
			Title = command.Title,
			Description = command.Description ?? string.Empty
		};

		_repository.UpdateAsync(Arg.Any<IssueDto>(), Arg.Any<CancellationToken>())
			.Returns(Result.Ok(updatedIssue));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeTrue();
		result.Value!.Title.Should().Be("Updated Title");
		result.Value!.Description.Should().Be("Updated Description");

		await _repository.Received(1).GetByIdAsync(issueId, Arg.Any<CancellationToken>());
		await _repository.Received(1).UpdateAsync(Arg.Is<IssueDto>(i =>
				i.Title == command.Title &&
				i.Description == command.Description), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_EmptyTitle_ReturnsValidationFailure()
	{
		// Arrange
		var command = new UpdateIssueCommand
		{
			Id = ObjectId.GenerateNewId(),
			Title = "",
			Description = "Some description"
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
		var command = new UpdateIssueCommand
		{
			Id = ObjectId.GenerateNewId(),
			Title = new string('A', 257),
			Description = "Some description"
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
		var command = new UpdateIssueCommand
		{
			Id = ObjectId.GenerateNewId(),
			Title = "Valid Title",
			Description = new string('X', 4097)
		};

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
	}

	[Fact]
	public async Task Handle_NonExistentIssue_ReturnsNotFoundFailure()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId();
		var command = new UpdateIssueCommand
		{
			Id = issueId,
			Title = "Updated Title",
			Description = "Updated Description"
		};

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(Result.Fail<IssueDto>("Not found"));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.NotFound);
	}

	[Fact]
	public async Task Handle_ArchivedIssue_ReturnsConflictFailure()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId();
		var archivedIssue = new IssueDto(
			issueId,
			"Archived Issue",
			"This is archived",
			DateTime.UtcNow.AddDays(-1),
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			true,
			UserDto.Empty,
			false,
			false);

		var command = new UpdateIssueCommand
		{
			Id = issueId,
			Title = "Updated Title",
			Description = "Updated Description"
		};

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok(archivedIssue));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Conflict);
	}

	[Fact]
	public async Task Handle_IdempotentUpdate_ReturnsUpdatedIssue()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId();
		var existingIssue = new IssueDto(
			issueId,
			"Same Title",
			"Same Description",
			DateTime.UtcNow.AddDays(-1),
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			false,
			UserDto.Empty,
			false,
			false);

		var command = new UpdateIssueCommand
		{
			Id = issueId,
			Title = "Same Title",
			Description = "Same Description"
		};

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok(existingIssue));

		var updatedIssue = existingIssue with { };

		_repository.UpdateAsync(Arg.Any<IssueDto>(), Arg.Any<CancellationToken>())
			.Returns(Result.Ok(updatedIssue));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeTrue();
		result.Value!.Title.Should().Be("Same Title");
	}

	[Fact]
	public async Task Handle_NullDescription_UsesEmptyString()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId();
		var existingIssue = new IssueDto(
			issueId,
			"Original Title",
			"Original Description",
			DateTime.UtcNow.AddDays(-1),
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			false,
			UserDto.Empty,
			false,
			false);

		var command = new UpdateIssueCommand
		{
			Id = issueId,
			Title = "Updated Title",
			Description = null
		};

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok(existingIssue));

		var updatedIssue = existingIssue with
		{
			Title = command.Title,
			Description = string.Empty
		};

		_repository.UpdateAsync(Arg.Any<IssueDto>(), Arg.Any<CancellationToken>())
			.Returns(Result.Ok(updatedIssue));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Value!.Description.Should().BeEmpty();
	}
}
