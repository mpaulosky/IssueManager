// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateIssueHandlerTests.cs
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

using Shared.Abstractions;
using Shared.DTOs;
using Shared.Validators;
using MongoDB.Bson;
using NSubstitute;

namespace Tests.Unit.Handlers.Issues;

/// <summary>
/// Unit tests for CreateIssueHandler.
/// </summary>
public class CreateIssueHandlerTests
{
	private readonly IIssueRepository _repository;
	private readonly CreateIssueValidator _validator;
	private readonly CreateIssueHandler _handler;

	public CreateIssueHandlerTests()
	{
		_repository = Substitute.For<IIssueRepository>();
		_validator = new CreateIssueValidator();
		_handler = new CreateIssueHandler(_repository, _validator);
	}

	[Fact]
	public async Task Handle_ValidCommand_ReturnsCreatedIssue()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = "New Issue",
			Description = "New issue description"
		};

		var createdIssue = new IssueDto(
			ObjectId.GenerateNewId(),
			command.Title,
			command.Description ?? string.Empty,
			DateTime.UtcNow,
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			false,
			UserDto.Empty,
			false,
			false);

		_repository.CreateAsync(Arg.Any<IssueDto>(), Arg.Any<CancellationToken>())
			.Returns(Result.Ok(createdIssue));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Title.Should().Be(command.Title);
		result.Description.Should().Be(command.Description);
		await _repository.Received(1).CreateAsync(Arg.Any<IssueDto>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_EmptyTitle_ThrowsValidationException()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = "",
			Description = "Description"
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Title*required*");
	}

	[Fact]
	public async Task Handle_TitleTooLong_ThrowsValidationException()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = new string('A', 201),
			Description = "Description"
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Title*200 characters*");
	}

	[Fact]
	public async Task Handle_DescriptionTooLong_ThrowsValidationException()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = "Valid Title",
			Description = new string('X', 5001)
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Description*5000 characters*");
	}

	[Fact]
	public async Task Handle_NullDescription_UsesEmptyString()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = "Valid Title",
			Description = null
		};

		var createdIssue = new IssueDto(
			ObjectId.GenerateNewId(),
			command.Title,
			string.Empty,
			DateTime.UtcNow,
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			false,
			UserDto.Empty,
			false,
			false);

		_repository.CreateAsync(Arg.Any<IssueDto>(), Arg.Any<CancellationToken>())
			.Returns(Result.Ok(createdIssue));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Description.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_ValidCommand_PassesCancellationToken()
	{
		// Arrange
		var command = new CreateIssueCommand
		{
			Title = "Test Issue",
			Description = "Test description"
		};

		var cancellationToken = new CancellationToken();
		var createdIssue = new IssueDto(
			ObjectId.GenerateNewId(),
			command.Title,
			command.Description ?? string.Empty,
			DateTime.UtcNow,
			null,
			UserDto.Empty,
			CategoryDto.Empty,
			StatusDto.Empty,
			false,
			UserDto.Empty,
			false,
			false);

		_repository.CreateAsync(Arg.Any<IssueDto>(), cancellationToken)
			.Returns(Result.Ok(createdIssue));

		// Act
		await _handler.Handle(command, cancellationToken);

		// Assert
		await _repository.Received(1).CreateAsync(Arg.Any<IssueDto>(), cancellationToken);
	}
}
