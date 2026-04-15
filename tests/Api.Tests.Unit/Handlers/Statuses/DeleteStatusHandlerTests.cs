// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteStatusHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =======================================================

using Api.Data.Interfaces;

namespace Api.Handlers.Statuses;

/// <summary>
/// Unit tests for DeleteStatusHandler (soft-delete via Archived).
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteStatusHandlerTests
{
	private readonly IStatusRepository _repository;
	private readonly DeleteStatusHandler _handler;

	public DeleteStatusHandlerTests()
	{
		_repository = Substitute.For<IStatusRepository>();
		_handler = new DeleteStatusHandler(_repository);
	}

	[Fact]
	public async Task Handle_ValidStatus_SetsIsArchivedToTrue()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		var status = new StatusDto(
			statusId,
			"Test Status",
			"Test Description",
			DateTime.UtcNow,
			null,
			false,
			UserDto.Empty);

		var command = new DeleteStatusCommand { Id = statusId };

		_repository.GetByIdAsync(statusId, Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Ok(status));

		_repository.ArchiveAsync(statusId, Arg.Any<CancellationToken>())
			.Returns(Result.Ok());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().BeTrue();
		await _repository.Received(1).GetByIdAsync(statusId, Arg.Any<CancellationToken>());
		await _repository.Received(1).ArchiveAsync(statusId, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_NonExistentStatus_ReturnsNotFoundResult()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		var command = new DeleteStatusCommand { Id = statusId };

		_repository.GetByIdAsync(statusId, Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Fail("Not found"));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.NotFound);
	}

	[Fact]
	public async Task Handle_AlreadyArchivedStatus_IsIdempotent()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		var archivedStatus = new StatusDto(
			statusId,
			"Archived Status",
			"Already archived",
			DateTime.UtcNow,
			null,
			true,
			UserDto.Empty);

		var command = new DeleteStatusCommand { Id = statusId };

		_repository.GetByIdAsync(statusId, Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Ok(archivedStatus));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().BeTrue();
		await _repository.Received(1).GetByIdAsync(statusId, Arg.Any<CancellationToken>());
		await _repository.DidNotReceive().ArchiveAsync(Arg.Any<ObjectId>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_EmptyId_ReturnsValidationFailure()
	{
		// Arrange
		var command = new DeleteStatusCommand { Id = ObjectId.Empty };

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
	}

	[Fact]
	public async Task Handle_RepositoryArchiveFails_ReturnsFailure()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		var status = new StatusDto(
			statusId,
			"Test Status",
			"Test Description",
			DateTime.UtcNow,
			null,
			false,
			UserDto.Empty);

		var command = new DeleteStatusCommand { Id = statusId };

		_repository.GetByIdAsync(statusId, Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Ok(status));

		_repository.ArchiveAsync(statusId, Arg.Any<CancellationToken>())
			.Returns(Result.Fail("Archive failed"));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Archive failed");
	}

	[Fact]
	public async Task Handle_ValidStatus_PassesCancellationToken()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		var cancellationToken = new CancellationToken();
		var status = new StatusDto(
			statusId,
			"Test Status",
			"Test Description",
			DateTime.UtcNow,
			null,
			false,
			UserDto.Empty);

		var command = new DeleteStatusCommand { Id = statusId };

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
