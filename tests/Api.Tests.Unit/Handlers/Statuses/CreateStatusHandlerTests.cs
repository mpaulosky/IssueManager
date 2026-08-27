// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateStatusHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =======================================================

using Api.Data.Interfaces;

namespace Api.Handlers.Statuses;

/// <summary>
/// Unit tests for CreateStatusHandler.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreateStatusHandlerTests
{
	private readonly IStatusRepository _repository;
	private readonly CreateStatusValidator _validator;
	private readonly CreateStatusHandler _handler;

	public CreateStatusHandlerTests()
	{
		_repository = Substitute.For<IStatusRepository>();
		_validator = new CreateStatusValidator();
		_handler = new CreateStatusHandler(_repository, _validator);
	}

	[Fact]
	public async Task Handle_ValidCommand_ReturnsCreatedStatus()
	{
		// Arrange
		var command = new CreateStatusCommand
		{
			StatusName = "Open",
			StatusDescription = "Issue is open"
		};

		_repository.CreateAsync(Arg.Any<StatusDto>(), Arg.Any<CancellationToken>())
			.Returns(Result.Ok(new StatusDto(ObjectId.GenerateNewId(), command.StatusName, command.StatusDescription, DateTime.UtcNow, null, false, UserDto.Empty)));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeTrue();
		result.Value!.StatusName.Should().Be(command.StatusName);
		result.Value!.StatusDescription.Should().Be(command.StatusDescription);
		await _repository.Received(1).CreateAsync(Arg.Is<StatusDto>(s =>
			s.StatusName == command.StatusName &&
			s.StatusDescription == command.StatusDescription), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_EmptyStatusName_ReturnsValidationFailure()
	{
		// Arrange
		var command = new CreateStatusCommand
		{
			StatusName = "",
			StatusDescription = "Description"
		};

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
		result.Error.Should().Contain("Status name").And.Contain("required");
	}

	[Fact]
	public async Task Handle_StatusNameTooShort_ReturnsValidationFailure()
	{
		// Arrange
		var command = new CreateStatusCommand
		{
			StatusName = "A",
			StatusDescription = "Description"
		};

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
		result.Error.Should().Contain("Status name").And.Contain("at least 2 characters");
	}

	[Fact]
	public async Task Handle_StatusNameTooLong_ReturnsValidationFailure()
	{
		// Arrange
		var command = new CreateStatusCommand
		{
			StatusName = new string('A', 101),
			StatusDescription = "Description"
		};

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
		result.Error.Should().Contain("Status name").And.Contain("100 characters");
	}

	[Fact]
	public async Task Handle_StatusDescriptionTooLong_ReturnsValidationFailure()
	{
		// Arrange
		var command = new CreateStatusCommand
		{
			StatusName = "Valid Name",
			StatusDescription = new string('X', 501)
		};

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Validation);
		result.Error.Should().Contain("Status description").And.Contain("500 characters");
	}

	[Fact]
	public async Task Handle_NullStatusDescription_UsesEmptyString()
	{
		// Arrange
		var command = new CreateStatusCommand
		{
			StatusName = "Valid Name",
			StatusDescription = null
		};

		_repository.CreateAsync(Arg.Any<StatusDto>(), Arg.Any<CancellationToken>())
			.Returns(Result.Ok(new StatusDto(ObjectId.GenerateNewId(), command.StatusName, string.Empty, DateTime.UtcNow, null, false, UserDto.Empty)));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Value!.StatusDescription.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_RepositoryFails_ReturnsFailureResult()
	{
		// Arrange
		var command = new CreateStatusCommand
		{
			StatusName = "Valid Name",
			StatusDescription = "Description"
		};

		_repository.CreateAsync(Arg.Any<StatusDto>(), Arg.Any<CancellationToken>())
			.Returns(Result.Fail<StatusDto>("Database error"));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Database error");
	}
}
