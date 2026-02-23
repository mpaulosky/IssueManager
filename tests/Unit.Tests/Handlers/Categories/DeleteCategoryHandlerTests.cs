// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteCategoryHandlerTests.cs
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
/// Unit tests for DeleteCategoryHandler (soft-delete via Archived).
/// </summary>
public class DeleteCategoryHandlerTests
{
	private readonly ICategoryRepository _repository;
	private readonly DeleteCategoryValidator _validator;
	private readonly DeleteCategoryHandler _handler;

	public DeleteCategoryHandlerTests()
	{
		_repository = Substitute.For<ICategoryRepository>();
		_validator = new DeleteCategoryValidator();
		_handler = new DeleteCategoryHandler(_repository, _validator);
	}

	[Fact]
	public async Task Handle_ValidCategory_SetsIsArchivedToTrue()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var category = new Category
		{
			Id = categoryId,
			CategoryName = "Bug",
			Archived = false
		};

		var command = new DeleteCategoryCommand { Id = categoryId.ToString() };

		_repository.GetAsync(categoryId)
			.Returns(Result<Category>.Ok(category));

		_repository.ArchiveAsync(category)
			.Returns(Result.Ok());

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().BeTrue();
		await _repository.Received(1).GetAsync(categoryId);
		await _repository.Received(1).ArchiveAsync(category);
	}

	[Fact]
	public async Task Handle_NonExistentCategory_ThrowsNotFoundException()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var command = new DeleteCategoryCommand { Id = categoryId.ToString() };

		_repository.GetAsync(categoryId)
			.Returns(Result<Category>.Fail("Not found"));

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>()
			.WithMessage($"*{categoryId}*");
	}

	[Fact]
	public async Task Handle_AlreadyArchivedCategory_IsIdempotent()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var archivedCategory = new Category
		{
			Id = categoryId,
			CategoryName = "Archived",
			Archived = true
		};

		var command = new DeleteCategoryCommand { Id = categoryId.ToString() };

		_repository.GetAsync(categoryId)
			.Returns(Result<Category>.Ok(archivedCategory));

		// Act
		var result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().BeTrue();
		await _repository.Received(1).GetAsync(categoryId);
		await _repository.DidNotReceive().ArchiveAsync(Arg.Any<Category>());
	}

	[Fact]
	public async Task Handle_InvalidId_ThrowsValidationException()
	{
		// Arrange
		var command = new DeleteCategoryCommand { Id = "" };

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>()
			.WithMessage("*Category ID*required*");
	}

	[Fact]
	public async Task Handle_InvalidObjectId_ThrowsNotFoundException()
	{
		// Arrange
		var command = new DeleteCategoryCommand { Id = "invalid-objectid" };

		// Act
		Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<NotFoundException>();
	}

	[Fact]
	public async Task Handle_ValidCategory_PassesCancellationToken()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var cancellationToken = new CancellationToken();
		var category = new Category
		{
			Id = categoryId,
			CategoryName = "Test",
			Archived = false
		};

		var command = new DeleteCategoryCommand { Id = categoryId.ToString() };

		_repository.GetAsync(categoryId)
			.Returns(Result<Category>.Ok(category));

		_repository.ArchiveAsync(category)
			.Returns(Result.Ok());

		// Act
		await _handler.Handle(command, cancellationToken);

		// Assert
		await _repository.Received(1).GetAsync(categoryId);
		await _repository.Received(1).ArchiveAsync(category);
	}
}
