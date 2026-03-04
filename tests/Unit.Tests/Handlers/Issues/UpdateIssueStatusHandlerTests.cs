// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateIssueStatusHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

using Unit.Builders;

namespace Unit.Handlers.Issues;

/// <summary>
/// Unit tests for UpdateIssueStatusHandler.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateIssueStatusHandlerTests
{
	private readonly IIssueRepository _repository;
	private readonly UpdateIssueStatusValidator _validator;
	private readonly UpdateIssueStatusHandler _handler;

	public UpdateIssueStatusHandlerTests()
	{
		_repository = Substitute.For<IIssueRepository>();
		_validator = new UpdateIssueStatusValidator();
		_handler = new UpdateIssueStatusHandler(_repository, _validator);
	}

	[Fact]
	public async Task Handle_ValidCommand_ReturnsUpdatedIssue()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId().ToString();
		var existingIssue = IssueBuilder.Default()
			.WithId(issueId)
			.WithStatus(new StatusDto(ObjectId.GenerateNewId(), "Open", "Issue is open", DateTime.UtcNow, null, false, UserDto.Empty))
			.Build();

		var newStatus = new StatusDto(ObjectId.GenerateNewId(), "Closed", "Issue is closed", DateTime.UtcNow, null, false, UserDto.Empty);
		var command = new UpdateIssueStatusCommand
		{
			IssueId = issueId,
			Status = newStatus
		};

		_repository.GetByIdAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>())
			.Returns(Result.Ok(existingIssue));

		var updatedIssue = existingIssue with { Status = newStatus };
		_repository.UpdateAsync(Arg.Any<IssueDto>(), Arg.Any<CancellationToken>())
			.Returns(Result.Ok(updatedIssue));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result!.Status.StatusName.Should().Be("Closed");
		await _repository.Received(1).GetByIdAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>());
		await _repository.Received(1).UpdateAsync(Arg.Is<IssueDto>(i => i.Status.StatusName == "Closed"), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_NonExistentIssue_ReturnsNull()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId().ToString();
		var command = new UpdateIssueStatusCommand
		{
			IssueId = issueId,
			Status = new StatusDto(ObjectId.GenerateNewId(), "Closed", "Issue is closed", DateTime.UtcNow, null, false, UserDto.Empty)
		};

		_repository.GetByIdAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>())
			.Returns(Result.Fail<IssueDto>("Not found"));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().BeNull();
		await _repository.Received(1).GetByIdAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>());
		await _repository.DidNotReceive().UpdateAsync(Arg.Any<IssueDto>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_EmptyIssueId_ThrowsValidationException()
	{
		// Arrange
		var command = new UpdateIssueStatusCommand
		{
			IssueId = "",
			Status = new StatusDto(ObjectId.GenerateNewId(), "Closed", "Issue is closed", DateTime.UtcNow, null, false, UserDto.Empty)
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Issue ID*required*");
	}

	[Fact]
	public async Task Handle_EmptyStatusName_ThrowsValidationException()
	{
		// Arrange
		var command = new UpdateIssueStatusCommand
		{
			IssueId = ObjectId.GenerateNewId().ToString(),
			Status = new StatusDto(ObjectId.GenerateNewId(), "", "Description", DateTime.UtcNow, null, false, UserDto.Empty)
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Status name*required*");
	}

	[Fact]
	public async Task Handle_ValidCommand_PassesCancellationToken()
	{
		// Arrange
		var issueId = ObjectId.GenerateNewId().ToString();
		var cancellationToken = new CancellationToken();
		var existingIssue = IssueBuilder.Default().WithId(issueId).Build();
		var newStatus = new StatusDto(ObjectId.GenerateNewId(), "In Progress", "Work in progress", DateTime.UtcNow, null, false, UserDto.Empty);

		var command = new UpdateIssueStatusCommand
		{
			IssueId = issueId,
			Status = newStatus
		};

		_repository.GetByIdAsync(Arg.Any<ObjectId>(), cancellationToken)
			.Returns(Result.Ok(existingIssue));

		var updatedIssue = existingIssue with { Status = newStatus };
		_repository.UpdateAsync(Arg.Any<IssueDto>(), cancellationToken)
			.Returns(Result.Ok(updatedIssue));

		// Act
		await _handler.Handle(command, cancellationToken);

		// Assert
		await _repository.Received(1).GetByIdAsync(Arg.Any<ObjectId>(), cancellationToken);
		await _repository.Received(1).UpdateAsync(Arg.Any<IssueDto>(), cancellationToken);
	}
}
