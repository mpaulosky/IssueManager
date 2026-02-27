// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateStatusHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

using FluentAssertions;
using FluentValidation;
using Api.Data;
using Api.Handlers;
using Api.Handlers.Statuses;

using Shared.Abstractions;
using Shared.Exceptions;
using Shared.Models;
using Shared.Validators;
using MongoDB.Bson;
using NSubstitute;

namespace Tests.Unit.Handlers.Statuses;

/// <summary>
/// Unit tests for UpdateStatusHandler.
/// </summary>
public class UpdateStatusHandlerTests
{
	private readonly IStatusRepository _repository;
	private readonly UpdateStatusValidator _validator;
	private readonly UpdateStatusHandler _handler;

	public UpdateStatusHandlerTests()
	{
		_repository = Substitute.For<IStatusRepository>();
		_validator = new UpdateStatusValidator();
		_handler = new UpdateStatusHandler(_repository, _validator);
	}

	[Fact]
	public async Task Handle_ValidCommand_ReturnsUpdatedStatus()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		var existingStatus = new Status
		{
			Id = statusId,
			StatusName = "Old Name",
			StatusDescription = "Old Description"
		};

		var command = new UpdateStatusCommand
		{
			Id = statusId.ToString(),
			StatusName = "Updated Name",
			StatusDescription = "Updated Description"
		};

		_repository.GetAsync(statusId)
			.Returns(Result<Status>.Ok(existingStatus));

		_repository.UpdateAsync(statusId, Arg.Any<Status>())
			.Returns(Result.Ok());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.StatusName.Should().Be("Updated Name");
		result.StatusDescription.Should().Be("Updated Description");
		await _repository.Received(1).UpdateAsync(statusId, Arg.Is<Status>(s =>
				s.StatusName == command.StatusName &&
				s.StatusDescription == command.StatusDescription));
	}

	[Fact]
	public async Task Handle_EmptyStatusName_ThrowsValidationException()
	{
		// Arrange
		var command = new UpdateStatusCommand
		{
			Id = ObjectId.GenerateNewId().ToString(),
			StatusName = "",
			StatusDescription = "Description"
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Status name*required*");
	}

	[Fact]
	public async Task Handle_StatusNameTooLong_ThrowsValidationException()
	{
		// Arrange
		var command = new UpdateStatusCommand
		{
			Id = ObjectId.GenerateNewId().ToString(),
			StatusName = new string('A', 101),
			StatusDescription = "Description"
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Status name*100 characters*");
	}

	[Fact]
	public async Task Handle_NonExistentStatus_ThrowsNotFoundException()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		var command = new UpdateStatusCommand
		{
			Id = statusId.ToString(),
			StatusName = "Updated Name",
			StatusDescription = "Updated Description"
		};

		_repository.GetAsync(statusId)
			.Returns(Result<Status>.Fail("Not found"));

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>()
			.WithMessage($"*{statusId}*");
	}

	[Fact]
	public async Task Handle_InvalidObjectId_ThrowsNotFoundException()
	{
		// Arrange
		var command = new UpdateStatusCommand
		{
			Id = "invalid-objectid",
			StatusName = "Updated Name",
			StatusDescription = "Updated Description"
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>();
	}

	[Fact]
	public async Task Handle_RepositoryUpdateFails_ThrowsNotFoundException()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		var existingStatus = new Status
		{
			Id = statusId,
			StatusName = "Old Name"
		};

		var command = new UpdateStatusCommand
		{
			Id = statusId.ToString(),
			StatusName = "Updated Name",
			StatusDescription = "Updated Description"
		};

		_repository.GetAsync(statusId)
			.Returns(Result<Status>.Ok(existingStatus));

		_repository.UpdateAsync(statusId, Arg.Any<Status>())
			.Returns(Result.Fail("Update failed"));

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>()
			.WithMessage("*could not be updated*");
	}

	[Fact]
	public async Task Handle_NullDescription_UsesEmptyString()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		var existingStatus = new Status
		{
			Id = statusId,
			StatusName = "Old Name"
		};

		var command = new UpdateStatusCommand
		{
			Id = statusId.ToString(),
			StatusName = "Updated Name",
			StatusDescription = null
		};

		_repository.GetAsync(statusId)
			.Returns(Result<Status>.Ok(existingStatus));

		_repository.UpdateAsync(statusId, Arg.Any<Status>())
			.Returns(Result.Ok());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.StatusDescription.Should().BeEmpty();
	}
}
