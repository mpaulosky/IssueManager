// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateStatusHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

namespace Unit.Handlers.Statuses;

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
		result.StatusName.Should().Be("Updated Name");
		result.StatusDescription.Should().Be("Updated Description");
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
			Id = ObjectId.GenerateNewId(),
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
			Id = statusId,
			StatusName = "Updated Name",
			StatusDescription = "Updated Description"
		};

		_repository.GetByIdAsync(statusId, Arg.Any<CancellationToken>())
			.Returns(Result<StatusDto>.Fail("Not found"));

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>()
			.WithMessage($"*{statusId}*");
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
		result.StatusDescription.Should().BeEmpty();
	}
}
