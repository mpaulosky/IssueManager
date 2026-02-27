// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteStatusHandlerTests.cs
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
using Shared.DTOs;
using Shared.Exceptions;
using Shared.Validators;
using MongoDB.Bson;
using NSubstitute;

namespace Tests.Unit.Handlers.Statuses;

/// <summary>
/// Unit tests for DeleteStatusHandler (soft-delete via Archived).
/// </summary>
public class DeleteStatusHandlerTests
{
	private readonly IStatusRepository _repository;
	private readonly DeleteStatusValidator _validator;
	private readonly DeleteStatusHandler _handler;

	public DeleteStatusHandlerTests()
	{
		_repository = Substitute.For<IStatusRepository>();
		_validator = new DeleteStatusValidator();
		_handler = new DeleteStatusHandler(_repository, _validator);
	}

	[Fact]
	public async Task Handle_ValidStatus_SetsIsArchivedToTrue()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		var status = new StatusDto(
			statusId,
			"Open",
			string.Empty,
			DateTime.UtcNow,
			null,
			false,
			UserDto.Empty);

		var command = new DeleteStatusCommand { Id = statusId.ToString() };

		_repository.GetByIdAsync(statusId, Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Ok(status));

		_repository.ArchiveAsync(statusId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().BeTrue();
		await _repository.Received(1).GetByIdAsync(statusId, Arg.Any<CancellationToken>());
		await _repository.Received(1).ArchiveAsync(statusId, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_NonExistentStatus_ThrowsNotFoundException()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		var command = new DeleteStatusCommand { Id = statusId.ToString() };

		_repository.GetByIdAsync(statusId, Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Fail("Not found"));

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>()
			.WithMessage($"*{statusId}*");
	}

	[Fact]
	public async Task Handle_AlreadyArchivedStatus_IsIdempotent()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		var archivedStatus = new StatusDto(
			statusId,
			"Archived",
			string.Empty,
			DateTime.UtcNow,
			null,
			true,
			UserDto.Empty);

		var command = new DeleteStatusCommand { Id = statusId.ToString() };

		_repository.GetByIdAsync(statusId, Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Ok(archivedStatus));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().BeTrue();
		await _repository.Received(1).GetByIdAsync(statusId, Arg.Any<CancellationToken>());
		await _repository.DidNotReceive().ArchiveAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_InvalidId_ThrowsValidationException()
	{
		// Arrange
		var command = new DeleteStatusCommand { Id = "" };

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Status ID*required*");
	}

	[Fact]
	public async Task Handle_InvalidObjectId_ThrowsNotFoundException()
	{
		// Arrange
		var command = new DeleteStatusCommand { Id = "invalid-objectid" };

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>();
	}

	[Fact]
	public async Task Handle_ValidStatus_PassesCancellationToken()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		var cancellationToken = new CancellationToken();
		var status = new StatusDto(
			statusId,
			"Test",
			string.Empty,
			DateTime.UtcNow,
			null,
			false,
			UserDto.Empty);

		var command = new DeleteStatusCommand { Id = statusId.ToString() };

		_repository.GetByIdAsync(statusId, Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Ok(status));

		_repository.ArchiveAsync(statusId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok());

		// Act
		await _handler.Handle(command, cancellationToken);

		// Assert
		await _repository.Received(1).GetByIdAsync(statusId, Arg.Any<CancellationToken>());
		await _repository.Received(1).ArchiveAsync(statusId, Arg.Any<CancellationToken>());
	}
}
