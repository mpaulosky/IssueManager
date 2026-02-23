using FluentAssertions;
using Api.Data;
using Api.Handlers;
using Shared.DTOs;
using Shared.Exceptions;
using Shared.Validators;
using MongoDB.Bson;
using NSubstitute;

namespace Tests.Unit.Handlers.Issues;

/// <summary>
/// Unit tests for UpdateIssueHandler.
/// </summary>
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
		var issueId = ObjectId.GenerateNewId().ToString();
		var existingIssue = new IssueDto(
			ObjectId.Parse(issueId),
			"Original Title",
			"Original Description",
			DateTime.UtcNow.AddDays(-1),
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty);

		var command = new UpdateIssueCommand
		{
			Id = issueId,
			Title = "Updated Title",
			Description = "Updated Description"
		};

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(existingIssue);

		var updatedIssue = existingIssue with
		{
			Title = command.Title,
			Description = command.Description ?? string.Empty
		};

		_repository.UpdateAsync(Arg.Any<IssueDto>(), Arg.Any<CancellationToken>())
			.Returns(updatedIssue);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Title.Should().Be("Updated Title");
		result.Description.Should().Be("Updated Description");

		await _repository.Received(1).GetByIdAsync(issueId, Arg.Any<CancellationToken>());
		await _repository.Received(1).UpdateAsync(Arg.Is<IssueDto>(i =>
			i.Title == command.Title &&
			i.Description == command.Description), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_EmptyTitle_ThrowsValidationException()
	{
		// Arrange
		var command = new UpdateIssueCommand
		{
			Id = ObjectId.GenerateNewId().ToString(),
			Title = "",
			Description = "Some description"
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Title*required*");
	}

	[Fact]
	public async Task Handle_TitleTooLong_ThrowsValidationException()
	{
		// Arrange
		var command = new UpdateIssueCommand
		{
			Id = ObjectId.GenerateNewId().ToString(),
			Title = new string('A', 257),
			Description = "Some description"
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Title*256*");
	}

	[Fact]
	public async Task Handle_DescriptionTooLong_ThrowsValidationException()
	{
		// Arrange
		var command = new UpdateIssueCommand
		{
			Id = ObjectId.GenerateNewId().ToString(),
			Title = "Valid Title",
			Description = new string('X', 4097)
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Description*4096*");
	}

	[Fact]
	public async Task Handle_NonExistentIssue_ThrowsNotFoundException()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId().ToString();
		var command = new UpdateIssueCommand
		{
			Id = issueId,
			Title = "Updated Title",
			Description = "Updated Description"
		};

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns((IssueDto?)null);

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>()
			.WithMessage($"*{issueId}*");
	}

	[Fact]
	public async Task Handle_ArchivedIssue_ThrowsConflictException()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId().ToString();
		var archivedIssue = new IssueDto(
			ObjectId.Parse(issueId),
			"Archived Issue",
			"This is archived",
			DateTime.UtcNow.AddDays(-1),
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			Archived: true);

		var command = new UpdateIssueCommand
		{
			Id = issueId,
			Title = "Updated Title",
			Description = "Updated Description"
		};

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(archivedIssue);

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ConflictException>()
			.WithMessage("*archived*");
	}

	[Fact]
	public async Task Handle_IdempotentUpdate_ReturnsUpdatedIssue()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId().ToString();
		var existingIssue = new IssueDto(
			ObjectId.Parse(issueId),
			"Same Title",
			"Same Description",
			DateTime.UtcNow.AddDays(-1),
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty);

		var command = new UpdateIssueCommand
		{
			Id = issueId,
			Title = "Same Title",
			Description = "Same Description"
		};

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(existingIssue);

		var updatedIssue = existingIssue with { };

		_repository.UpdateAsync(Arg.Any<IssueDto>(), Arg.Any<CancellationToken>())
			.Returns(updatedIssue);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Title.Should().Be("Same Title");
	}

	[Fact]
	public async Task Handle_NullDescription_UsesEmptyString()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId().ToString();
		var existingIssue = new IssueDto(
			ObjectId.Parse(issueId),
			"Original Title",
			"Original Description",
			DateTime.UtcNow.AddDays(-1),
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty);

		var command = new UpdateIssueCommand
		{
			Id = issueId,
			Title = "Updated Title",
			Description = null
		};

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(existingIssue);

		var updatedIssue = existingIssue with
		{
			Title = command.Title,
			Description = string.Empty
		};

		_repository.UpdateAsync(Arg.Any<IssueDto>(), Arg.Any<CancellationToken>())
			.Returns(updatedIssue);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Description.Should().BeEmpty();
	}
}
