// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateCategoryHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

using FluentAssertions;
using FluentValidation;
using Api.Data;
using Api.Handlers;
using Shared.Abstractions;
using Shared.Exceptions;
using Shared.Models;
using Shared.Validators;
using MongoDB.Bson;
using NSubstitute;

namespace Tests.Unit.Handlers.Categories;

/// <summary>
/// Unit tests for UpdateCategoryHandler.
/// </summary>
public class UpdateCategoryHandlerTests
{
	private readonly ICategoryRepository _repository;
	private readonly UpdateCategoryValidator _validator;
	private readonly UpdateCategoryHandler _handler;

	public UpdateCategoryHandlerTests()
	{
		_repository = Substitute.For<ICategoryRepository>();
		_validator = new UpdateCategoryValidator();
		_handler = new UpdateCategoryHandler(_repository, _validator);
	}

	[Fact]
	public async Task Handle_ValidCommand_ReturnsUpdatedCategory()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var existingCategory = new Category
		{
			Id = categoryId,
			CategoryName = "Old Name",
			CategoryDescription = "Old Description"
		};

		var command = new UpdateCategoryCommand
		{
			Id = categoryId.ToString(),
			CategoryName = "Updated Name",
			CategoryDescription = "Updated Description"
		};

		_repository.GetAsync(categoryId)
			.Returns(Result<Category>.Ok(existingCategory));

		_repository.UpdateAsync(categoryId, Arg.Any<Category>())
			.Returns(Result.Ok());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.CategoryName.Should().Be("Updated Name");
		result.CategoryDescription.Should().Be("Updated Description");
		await _repository.Received(1).UpdateAsync(categoryId, Arg.Is<Category>(c =>
			c.CategoryName == command.CategoryName &&
			c.CategoryDescription == command.CategoryDescription));
	}

	[Fact]
	public async Task Handle_EmptyCategoryName_ThrowsValidationException()
	{
		// Arrange
		var command = new UpdateCategoryCommand
		{
			Id = ObjectId.GenerateNewId().ToString(),
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
	public async Task Handle_CategoryNameTooLong_ThrowsValidationException()
	{
		// Arrange
		var command = new UpdateCategoryCommand
		{
			Id = ObjectId.GenerateNewId().ToString(),
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
	public async Task Handle_NonExistentCategory_ThrowsNotFoundException()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var command = new UpdateCategoryCommand
		{
			Id = categoryId.ToString(),
			CategoryName = "Updated Name",
			CategoryDescription = "Updated Description"
		};

		_repository.GetAsync(categoryId)
			.Returns(Result<Category>.Fail("Not found"));

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>()
			.WithMessage($"*{categoryId}*");
	}

	[Fact]
	public async Task Handle_InvalidObjectId_ThrowsNotFoundException()
	{
		// Arrange
		var command = new UpdateCategoryCommand
		{
			Id = "invalid-objectid",
			CategoryName = "Updated Name",
			CategoryDescription = "Updated Description"
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
		var categoryId = ObjectId.GenerateNewId();
		var existingCategory = new Category
		{
			Id = categoryId,
			CategoryName = "Old Name"
		};

		var command = new UpdateCategoryCommand
		{
			Id = categoryId.ToString(),
			CategoryName = "Updated Name",
			CategoryDescription = "Updated Description"
		};

		_repository.GetAsync(categoryId)
			.Returns(Result<Category>.Ok(existingCategory));

		_repository.UpdateAsync(categoryId, Arg.Any<Category>())
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
		var categoryId = ObjectId.GenerateNewId();
		var existingCategory = new Category
		{
			Id = categoryId,
			CategoryName = "Old Name"
		};

		var command = new UpdateCategoryCommand
		{
			Id = categoryId.ToString(),
			CategoryName = "Updated Name",
			CategoryDescription = null
		};

		_repository.GetAsync(categoryId)
			.Returns(Result<Category>.Ok(existingCategory));

		_repository.UpdateAsync(categoryId, Arg.Any<Category>())
			.Returns(Result.Ok());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.CategoryDescription.Should().BeEmpty();
	}
}
