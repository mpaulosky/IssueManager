// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateStatusHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =======================================================

using Api.Data.Interfaces;

namespace Api.Handlers.Statuses;

/// <summary>
/// Unit tests for UpdateStatusHandler.
/// </summary>
[ExcludeFromCodeCoverage]
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
		var existingStatus = new StatusDto(
			statusId,
			"Old Name",
			"Old Description",
			DateTime.UtcNow,
			null,
			false,
			UserDto.Empty);

		var updatedStatus = existingStatus with
		{
			StatusName = "Updated Name",
			StatusDescription = "Updated Description"
		};

		var command = new UpdateStatusCommand
		{
			Id = statusId,
			StatusName = "Updated Name",
			StatusDescription = "Updated Description"
		};

		_repository.GetByIdAsync(statusId, Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Ok(existingStatus));

		_repository.UpdateAsync(Arg.Any<StatusDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Ok(updatedStatus));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value!.StatusName.Should().Be("Updated Name");
		result.Value.StatusDescription.Should().Be("Updated Description");
		await _repository.Received(1).UpdateAsync(Arg.Is<StatusDto>(s =>
				s.StatusName == command.StatusName &&
				s.StatusDescription == command.StatusDescription), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_EmptyStatusName_ThrowsValidationException()
	{
		// Arrange
		var command = new UpdateStatusCommand
		{
			Id = ObjectId.GenerateNewId(),
			StatusName = "",
			StatusDescription = "Description"
		};

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
	}

	[Fact]
	public async Task Handle_StatusNameTooLong_ThrowsValidationException()
	{
		// Arrange
		var command = new UpdateStatusCommand
		{
			Id = ObjectId.GenerateNewId(),
			StatusName = new string('A', 101),
			StatusDescription = "Description"
		};

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
	}

	[Fact]
	public async Task Handle_NonExistentStatus_ThrowsNotFoundException()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		var command = new UpdateStatusCommand
		{
			Id = statusId,
			StatusName = "Updated Name",
			StatusDescription = "Updated Description"
		};

		_repository.GetByIdAsync(statusId, Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Fail("Not found"));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.NotFound);
	}

	[Fact]
	public async Task Handle_RepositoryUpdateFails_ThrowsNotFoundException()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		var existingStatus = new StatusDto(
			statusId,
			"Old Name",
			string.Empty,
			DateTime.UtcNow,
			null,
			false,
			UserDto.Empty);

		var command = new UpdateStatusCommand
		{
			Id = statusId,
			StatusName = "Updated Name",
			StatusDescription = "Updated Description"
		};

		_repository.GetByIdAsync(statusId, Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Ok(existingStatus));

		_repository.UpdateAsync(Arg.Any<StatusDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Fail("Update failed"));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Update failed");
	}

	[Fact]
	public async Task Handle_NullDescription_UsesEmptyString()
	{
		// Arrange
		var statusId = ObjectId.GenerateNewId();
		var existingStatus = new StatusDto(
			statusId,
			"Old Name",
			string.Empty,
			DateTime.UtcNow,
			null,
			false,
			UserDto.Empty);

		var updatedStatus = existingStatus with
		{
			StatusName = "Updated Name",
			StatusDescription = string.Empty
		};

		var command = new UpdateStatusCommand
		{
			Id = statusId,
			StatusName = "Updated Name",
			StatusDescription = null
		};

		_repository.GetByIdAsync(statusId, Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Ok(existingStatus));

		_repository.UpdateAsync(Arg.Any<StatusDto>(), Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Ok(updatedStatus));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value!.StatusDescription.Should().BeEmpty();
	}
}
