using FluentAssertions;
using FluentValidation;
using IssueManager.Api.Data;
using IssueManager.Api.Handlers;
using global::Shared.Domain;
using global::Shared.Exceptions;
using IssueManager.Shared.Validators;
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
		_handler = new DeleteIssueHandler(_repository, new DeleteIssueValidator());
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
			Status: IssueStatus.Open,
			CreatedAt: DateTime.UtcNow.AddDays(-1),
			UpdatedAt: DateTime.UtcNow.AddDays(-1))
		{
			IsArchived = false
		};

		var command = new DeleteIssueCommand { Id = issueId };

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(existingIssue);

		_repository.ArchiveAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(true);

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		await _repository.Received(1).GetByIdAsync(issueId, Arg.Any<CancellationToken>());
		await _repository.Received(1).ArchiveAsync(issueId, Arg.Any<CancellationToken>());
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
			Status: IssueStatus.Open,
			CreatedAt: DateTime.UtcNow.AddDays(-1),
			UpdatedAt: DateTime.UtcNow.AddHours(-1))
		{
			IsArchived = true
		};

		var command = new DeleteIssueCommand { Id = issueId };

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(archivedIssue);

		// Act — should succeed idempotently without calling ArchiveAsync
		await _handler.Handle(command, CancellationToken.None);

		await _repository.Received(1).GetByIdAsync(issueId, Arg.Any<CancellationToken>());
		// Should NOT call ArchiveAsync since already archived
		await _repository.DidNotReceive().ArchiveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_ValidIssue_CallsArchive()
	{
		// Arrange
		var issueId = Guid.NewGuid().ToString();
		var existingIssue = new Issue(
			Id: issueId,
			Title: "Issue to Delete",
			Description: "This will be archived",
			Status: IssueStatus.Open,
			CreatedAt: DateTime.UtcNow.AddDays(-1),
			UpdatedAt: DateTime.UtcNow.AddHours(-2))
		{
			IsArchived = false
		};

		var command = new DeleteIssueCommand { Id = issueId };

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(existingIssue);

		_repository.ArchiveAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(true);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().BeTrue();
		await _repository.Received(1).ArchiveAsync(issueId, Arg.Any<CancellationToken>());
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
