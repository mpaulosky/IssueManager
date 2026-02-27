// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateIssueStatusHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

using FluentAssertions;
using FluentValidation;
using Api.Data;
using Api.Handlers;
using Api.Handlers.Issues;

using Shared.DTOs;
using Shared.Validators;
using MongoDB.Bson;
using NSubstitute;
using Tests.Unit.Builders;

namespace Tests.Unit.Handlers.Issues;

/// <summary>
/// Unit tests for UpdateIssueStatusHandler.
/// </summary>
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
			.WithStatus(new StatusDto("Open", "Issue is open"))
			.Build();

		var newStatus = new StatusDto("Closed", "Issue is closed");
		var command = new UpdateIssueStatusCommand
		{
			IssueId = issueId,
			Status = newStatus
		};

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns(existingIssue);

		var updatedIssue = existingIssue with { Status = newStatus };
		_repository.UpdateAsync(Arg.Any<IssueDto>(), Arg.Any<CancellationToken>())
			.Returns(updatedIssue);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result!.Status.StatusName.Should().Be("Closed");
		await _repository.Received(1).GetByIdAsync(issueId, Arg.Any<CancellationToken>());
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
			Status = new StatusDto("Closed", "Issue is closed")
		};

		_repository.GetByIdAsync(issueId, Arg.Any<CancellationToken>())
			.Returns((IssueDto?)null);

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().BeNull();
		await _repository.Received(1).GetByIdAsync(issueId, Arg.Any<CancellationToken>());
		await _repository.DidNotReceive().UpdateAsync(Arg.Any<IssueDto>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_EmptyIssueId_ThrowsValidationException()
	{
		// Arrange
		var command = new UpdateIssueStatusCommand
		{
			IssueId = "",
			Status = new StatusDto("Closed", "Issue is closed")
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
			Status = new StatusDto("", "Description")
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
		var newStatus = new StatusDto("In Progress", "Work in progress");

		var command = new UpdateIssueStatusCommand
		{
			IssueId = issueId,
			Status = newStatus
		};

		_repository.GetByIdAsync(issueId, cancellationToken)
			.Returns(existingIssue);

		var updatedIssue = existingIssue with { Status = newStatus };
		_repository.UpdateAsync(Arg.Any<IssueDto>(), cancellationToken)
			.Returns(updatedIssue);

		// Act
		await _handler.Handle(command, cancellationToken);

		// Assert
		await _repository.Received(1).GetByIdAsync(issueId, cancellationToken);
		await _repository.Received(1).UpdateAsync(Arg.Any<IssueDto>(), cancellationToken);
	}
}
