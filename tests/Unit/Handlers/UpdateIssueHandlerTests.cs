using FluentAssertions;
using IssueManager.Api.Data;
using IssueManager.Api.Handlers;
using IssueManager.Shared.Domain.Models;
using IssueManager.Shared.Validators;
using NSubstitute;

namespace IssueManager.Tests.Unit.Handlers;

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
		var issueId = Guid.NewGuid().ToString();
		var existingIssue = new Issue(
			Id: issueId,
			Title: "Original Title",
			Description: "Original Description",
			AuthorId: "user-123",
			CreatedAt: DateTime.UtcNow.AddDays(-1))
		{
			UpdatedAt = DateTime.UtcNow.AddDays(-1)
		};

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
			Description = command.Description,
			UpdatedAt = DateTime.UtcNow
		};

		_repository.UpdateAsync(Arg.Any<Issue>(), Arg.Any<CancellationToken>())
			.Returns(updatedIssue);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Title.Should().Be("Updated Title");
		result.Description.Should().Be("Updated Description");
		result.UpdatedAt.Should().NotBeNull();
		result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

		await _repository.Received(1).GetByIdAsync(issueId, Arg.Any<CancellationToken>());
		await _repository.Received(1).UpdateAsync(Arg.Is<Issue>(i =>
			i.Title == command.Title &&
			i.Description == command.Description), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_EmptyTitle_ThrowsValidationException()
	{
		// Arrange
		var command = new UpdateIssueCommand
		{
			Id = Guid.NewGuid().ToString(),
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
			Id = Guid.NewGuid().ToString(),
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
			Id = Guid.NewGuid().ToString(),
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
		var issueId = Guid.NewGuid().ToString();
		var command = new UpdateIssueCommand
		{
			Id = issueId,
			Title = "Updated Title",
			Description = "Updated Description"
		};

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns((Issue?)null);

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
		var issueId = Guid.NewGuid().ToString();
		var archivedIssue = new Issue(
			Id: issueId,
			Title: "Archived Issue",
			Description: "This is archived",
			AuthorId: "user-123",
			CreatedAt: DateTime.UtcNow.AddDays(-1))
		{
			IsArchived = true
		};

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
	public async Task Handle_IdempotentUpdate_UpdatesTimestamp()
	{
		// Arrange
		var issueId = Guid.NewGuid().ToString();
		var existingIssue = new Issue(
			Id: issueId,
			Title: "Same Title",
			Description: "Same Description",
			AuthorId: "user-123",
			CreatedAt: DateTime.UtcNow.AddDays(-1))
		{
			UpdatedAt = DateTime.UtcNow.AddHours(-1)
		};

		var command = new UpdateIssueCommand
		{
			Id = issueId,
			Title = "Same Title",
			Description = "Same Description"
		};

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(existingIssue);

		var updatedIssue = existingIssue with { UpdatedAt = DateTime.UtcNow };

		_repository.UpdateAsync(Arg.Any<Issue>(), Arg.Any<CancellationToken>())
			.Returns(updatedIssue);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.UpdatedAt.Should().NotBeNull();
		result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
		result.UpdatedAt.Should().BeAfter(existingIssue.UpdatedAt!.Value);
	}

	[Fact]
	public async Task Handle_NullDescription_AllowsNullValue()
	{
		// Arrange
		var issueId = Guid.NewGuid().ToString();
		var existingIssue = new Issue(
			Id: issueId,
			Title: "Original Title",
			Description: "Original Description",
			AuthorId: "user-123",
			CreatedAt: DateTime.UtcNow.AddDays(-1));

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
			Description = command.Description,
			UpdatedAt = DateTime.UtcNow
		};

		_repository.UpdateAsync(Arg.Any<Issue>(), Arg.Any<CancellationToken>())
			.Returns(updatedIssue);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Description.Should().BeNull();
	}
}
