using FluentAssertions;
using IssueManager.Api.Data;
using IssueManager.Api.Handlers;
using IssueManager.Shared.Domain.Models;
using NSubstitute;

namespace IssueManager.Tests.Unit.Handlers;

/// <summary>
/// Unit tests for DeleteIssueHandler (soft-delete via IsArchived).
/// </summary>
public class DeleteIssueHandlerTests
{
	private readonly IIssueRepository _repository;
	private readonly DeleteIssueHandler _handler;

	public DeleteIssueHandlerTests()
	{
		_repository = Substitute.For<IIssueRepository>();
		_handler = new DeleteIssueHandler(_repository);
	}

	[Fact]
	public async Task Handle_ValidIssue_SetsIsArchivedToTrue()
	{
		// Arrange
		var issueId = Guid.NewGuid().ToString();
		var existingIssue = new Issue(
			Id: issueId,
			Title: "Issue to Delete",
			Description: "This will be archived",
			AuthorId: "user-123",
			CreatedAt: DateTime.UtcNow.AddDays(-1))
		{
			IsArchived = false
		};

		var command = new DeleteIssueCommand { Id = issueId };

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(existingIssue);

		var archivedIssue = existingIssue with
		{
			IsArchived = true,
			UpdatedAt = DateTime.UtcNow
		};

		_repository.UpdateAsync(Arg.Any<Issue>(), Arg.Any<CancellationToken>())
			.Returns(archivedIssue);

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		await _repository.Received(1).GetByIdAsync(issueId, Arg.Any<CancellationToken>());
		await _repository.Received(1).UpdateAsync(
			Arg.Is<Issue>(i => i.IsArchived == true && i.Id == issueId),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_NonExistentIssue_ThrowsNotFoundException()
	{
		// Arrange
		var issueId = Guid.NewGuid().ToString();
		var command = new DeleteIssueCommand { Id = issueId };

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns((Issue?)null);

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>()
			.WithMessage($"*{issueId}*");
	}

	[Fact]
	public async Task Handle_AlreadyArchivedIssue_IsIdempotent()
	{
		// Arrange
		var issueId = Guid.NewGuid().ToString();
		var archivedIssue = new Issue(
			Id: issueId,
			Title: "Already Archived",
			Description: "Already archived",
			AuthorId: "user-123",
			CreatedAt: DateTime.UtcNow.AddDays(-1))
		{
			IsArchived = true,
			UpdatedAt = DateTime.UtcNow.AddHours(-1)
		};

		var command = new DeleteIssueCommand { Id = issueId };

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(archivedIssue);

		// Act & Assert - Should be idempotent (either succeed silently or throw)
		// Decision: Return success (204) without updating (idempotent)
		await _handler.Handle(command, CancellationToken.None);

		await _repository.Received(1).GetByIdAsync(issueId, Arg.Any<CancellationToken>());
		// Should NOT call UpdateAsync since already archived
		await _repository.DidNotReceive().UpdateAsync(Arg.Any<Issue>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_ValidIssue_UpdatesTimestamp()
	{
		// Arrange
		var issueId = Guid.NewGuid().ToString();
		var existingIssue = new Issue(
			Id: issueId,
			Title: "Issue to Delete",
			Description: "This will be archived",
			AuthorId: "user-123",
			CreatedAt: DateTime.UtcNow.AddDays(-1))
		{
			IsArchived = false,
			UpdatedAt = DateTime.UtcNow.AddHours(-2)
		};

		var command = new DeleteIssueCommand { Id = issueId };

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(existingIssue);

		var archivedIssue = existingIssue with
		{
			IsArchived = true,
			UpdatedAt = DateTime.UtcNow
		};

		_repository.UpdateAsync(Arg.Any<Issue>(), Arg.Any<CancellationToken>())
			.Returns(archivedIssue);

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		await _repository.Received(1).UpdateAsync(
			Arg.Is<Issue>(i => i.UpdatedAt != null && i.UpdatedAt > existingIssue.UpdatedAt),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_InvalidId_ThrowsValidationException()
	{
		// Arrange
		var command = new DeleteIssueCommand { Id = "" };

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Id*required*");
	}
}
