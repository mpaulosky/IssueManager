// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteIssueHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit.Tests
// =======================================================
namespace Unit.Handlers.Issues;

/// <summary>
/// Unit tests for DeleteIssueHandler (soft-delete via Archived).
/// </summary>
[ExcludeFromCodeCoverage]
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
		var issueId = ObjectId.GenerateNewId().ToString();
		var objectId = ObjectId.Parse(issueId);
		var existingIssue = new IssueDto(
			objectId,
			"Issue to Delete",
			"This will be archived",
			DateTime.UtcNow.AddDays(-1),
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			Archived: false,
			UserDto.Empty,
			ApprovedForRelease: false,
			Rejected: false);

		var command = new DeleteIssueCommand { Id = issueId };

		_repository.GetByIdAsync(objectId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok(existingIssue));

		_repository.ArchiveAsync(objectId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok());

		// Act
		await _handler.Handle(command, CancellationToken.None);

		// Assert
		await _repository.Received(1).GetByIdAsync(objectId, Arg.Any<CancellationToken>());
		await _repository.Received(1).ArchiveAsync(objectId, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_NonExistentIssue_ThrowsNotFoundException()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId().ToString();
		var objectId = ObjectId.Parse(issueId);
		var command = new DeleteIssueCommand { Id = issueId };

		_repository.GetByIdAsync(objectId, Arg.Any<CancellationToken>())
			.Returns(Result.Fail<IssueDto>("not found"));

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
		var issueId = ObjectId.GenerateNewId().ToString();
		var objectId = ObjectId.Parse(issueId);
		var archivedIssue = new IssueDto(
			objectId,
			"Already Archived",
			"Already archived",
			DateTime.UtcNow.AddDays(-1),
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			Archived: true,
			UserDto.Empty,
			ApprovedForRelease: false,
			Rejected: false);

		var command = new DeleteIssueCommand { Id = issueId };

		_repository.GetByIdAsync(objectId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok(archivedIssue));

		// Act — should succeed idempotently without calling ArchiveAsync
		await _handler.Handle(command, CancellationToken.None);

		await _repository.Received(1).GetByIdAsync(objectId, Arg.Any<CancellationToken>());
		// Should NOT call ArchiveAsync since already archived
		await _repository.DidNotReceive().ArchiveAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_ValidIssue_CallsArchive()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId().ToString();
		var objectId = ObjectId.Parse(issueId);
		var existingIssue = new IssueDto(
			objectId,
			"Issue to Delete",
			"This will be archived",
			DateTime.UtcNow.AddDays(-1),
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			Archived: false,
			UserDto.Empty,
			ApprovedForRelease: false,
			Rejected: false);

		var command = new DeleteIssueCommand { Id = issueId };

		_repository.GetByIdAsync(objectId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok(existingIssue));

		_repository.ArchiveAsync(objectId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().BeTrue();
		await _repository.Received(1).ArchiveAsync(objectId, Arg.Any<CancellationToken>());
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
