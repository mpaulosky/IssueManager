// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateStatusHandlerTests.cs
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
using Shared.Validators;
using NSubstitute;

namespace Tests.Unit.Handlers.Statuses;

/// <summary>
/// Unit tests for CreateStatusHandler.
/// </summary>
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

		_repository.CreateAsync(Arg.Any<Shared.Models.Status>())
			.Returns(Result.Ok());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.StatusName.Should().Be(command.StatusName);
		result.StatusDescription.Should().Be(command.StatusDescription);
		await _repository.Received(1).CreateAsync(Arg.Is<Shared.Models.Status>(s =>
			s.StatusName == command.StatusName &&
			s.StatusDescription == command.StatusDescription));
	}

	[Fact]
	public async Task Handle_EmptyStatusName_ThrowsValidationException()
	{
		// Arrange
		var command = new CreateStatusCommand
		{
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
	public async Task Handle_StatusNameTooShort_ThrowsValidationException()
	{
		// Arrange
		var command = new CreateStatusCommand
		{
			StatusName = "A",
			StatusDescription = "Description"
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Status name*at least 2 characters*");
	}

	[Fact]
	public async Task Handle_StatusNameTooLong_ThrowsValidationException()
	{
		// Arrange
		var command = new CreateStatusCommand
		{
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
	public async Task Handle_StatusDescriptionTooLong_ThrowsValidationException()
	{
		// Arrange
		var command = new CreateStatusCommand
		{
			StatusName = "Valid Name",
			StatusDescription = new string('X', 501)
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Status description*500 characters*");
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

		_repository.CreateAsync(Arg.Any<Shared.Models.Status>())
			.Returns(Result.Ok());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.StatusDescription.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_RepositoryFails_ThrowsInvalidOperationException()
	{
		// Arrange
		var command = new CreateStatusCommand
		{
			StatusName = "Valid Name",
			StatusDescription = "Description"
		};

		_repository.CreateAsync(Arg.Any<Shared.Models.Status>())
			.Returns(Result.Fail("Database error"));

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("Database error");
	}
}
