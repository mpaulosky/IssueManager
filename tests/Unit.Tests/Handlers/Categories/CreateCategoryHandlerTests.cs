// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateCategoryHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

using FluentAssertions;
using FluentValidation;
using Api.Data;
using Api.Handlers;
using Api.Handlers.Categories;

using Shared.Abstractions;
using Shared.Validators;
using NSubstitute;

namespace Tests.Unit.Handlers.Categories;

/// <summary>
/// Unit tests for CreateCategoryHandler.
/// </summary>
public class CreateCategoryHandlerTests
{
	private readonly ICategoryRepository _repository;
	private readonly CreateCategoryValidator _validator;
	private readonly CreateCategoryHandler _handler;

	public CreateCategoryHandlerTests()
	{
		_repository = Substitute.For<ICategoryRepository>();
		_validator = new CreateCategoryValidator();
		_handler = new CreateCategoryHandler(_repository, _validator);
	}

	[Fact]
	public async Task Handle_ValidCommand_ReturnsCreatedCategory()
	{
		// Arrange
		var command = new CreateCategoryCommand
		{
			CategoryName = "Bug",
			CategoryDescription = "Bug reports"
		};

		_repository.CreateAsync(Arg.Any<Shared.Models.Category>())
			.Returns(Result.Ok());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.CategoryName.Should().Be(command.CategoryName);
		result.CategoryDescription.Should().Be(command.CategoryDescription);
		await _repository.Received(1).CreateAsync(Arg.Is<Shared.Models.Category>(c =>
			c.CategoryName == command.CategoryName &&
			c.CategoryDescription == command.CategoryDescription));
	}

	[Fact]
	public async Task Handle_EmptyCategoryName_ThrowsValidationException()
	{
		// Arrange
		var command = new CreateCategoryCommand
		{
			CategoryName = "",
			CategoryDescription = "Description"
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Category name*required*");
	}

	[Fact]
	public async Task Handle_CategoryNameTooShort_ThrowsValidationException()
	{
		// Arrange
		var command = new CreateCategoryCommand
		{
			CategoryName = "A",
			CategoryDescription = "Description"
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Category name*at least 2 characters*");
	}

	[Fact]
	public async Task Handle_CategoryNameTooLong_ThrowsValidationException()
	{
		// Arrange
		var command = new CreateCategoryCommand
		{
			CategoryName = new string('A', 101),
			CategoryDescription = "Description"
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Category name*100 characters*");
	}

	[Fact]
	public async Task Handle_CategoryDescriptionTooLong_ThrowsValidationException()
	{
		// Arrange
		var command = new CreateCategoryCommand
		{
			CategoryName = "Valid Name",
			CategoryDescription = new string('X', 501)
		};

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Category description*500 characters*");
	}

	[Fact]
	public async Task Handle_NullCategoryDescription_UsesEmptyString()
	{
		// Arrange
		var command = new CreateCategoryCommand
		{
			CategoryName = "Valid Name",
			CategoryDescription = null
		};

		_repository.CreateAsync(Arg.Any<Shared.Models.Category>())
			.Returns(Result.Ok());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.CategoryDescription.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_RepositoryFails_ThrowsInvalidOperationException()
	{
		// Arrange
		var command = new CreateCategoryCommand
		{
			CategoryName = "Valid Name",
			CategoryDescription = "Description"
		};

		_repository.CreateAsync(Arg.Any<Shared.Models.Category>())
			.Returns(Result.Fail("Database error"));

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("Database error");
	}
}
