// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     GetCategoryHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

using FluentAssertions;
using Api.Data;
using Api.Handlers;
using Api.Handlers.Categories;

using Shared.Abstractions;
using Shared.Models;
using MongoDB.Bson;
using NSubstitute;

namespace Tests.Unit.Handlers.Categories;

/// <summary>
/// Unit tests for GetCategoryHandler.
/// </summary>
public class GetCategoryHandlerTests
{
	private readonly ICategoryRepository _repository;
	private readonly GetCategoryHandler _handler;

	public GetCategoryHandlerTests()
	{
		_repository = Substitute.For<ICategoryRepository>();
		_handler = new GetCategoryHandler(_repository);
	}

	[Fact]
	public async Task Handle_ValidCategoryId_ReturnsCategory()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var category = new Category
		{
			Id = categoryId,
			CategoryName = "Bug",
			CategoryDescription = "Bug reports",
			DateCreated = DateTime.UtcNow
		};

		_repository.GetAsync(categoryId)
			.Returns(Result<Category>.Ok(category));

		var query = new GetCategoryQuery(categoryId.ToString());

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result!.CategoryName.Should().Be("Bug");
		result.CategoryDescription.Should().Be("Bug reports");
		await _repository.Received(1).GetAsync(categoryId);
	}

	[Fact]
	public async Task Handle_NonExistentCategoryId_ReturnsNull()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		_repository.GetAsync(categoryId)
			.Returns(Result<Category>.Fail("Not found"));

		var query = new GetCategoryQuery(categoryId.ToString());

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public async Task Handle_EmptyCategoryId_ThrowsArgumentException()
	{
		// Arrange
		var query = new GetCategoryQuery("");

		// Act
		Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("*Category ID cannot be empty*");
	}

	[Fact]
	public async Task Handle_WhitespaceCategoryId_ThrowsArgumentException()
	{
		// Arrange
		var query = new GetCategoryQuery("   ");

		// Act
		Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("*Category ID cannot be empty*");
	}

	[Fact]
	public async Task Handle_InvalidObjectId_ReturnsNull()
	{
		// Arrange
		var query = new GetCategoryQuery("invalid-object-id");

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeNull();
		await _repository.DidNotReceive().GetAsync(Arg.Any<ObjectId>());
	}

	[Fact]
	public async Task Handle_ValidCategoryId_PassesCancellationToken()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var cancellationToken = new CancellationToken();
		var category = new Category
		{
			Id = categoryId,
			CategoryName = "Feature",
			CategoryDescription = "Feature requests"
		};

		_repository.GetAsync(categoryId)
			.Returns(Result<Category>.Ok(category));

		var query = new GetCategoryQuery(categoryId.ToString());

		// Act
		await _handler.Handle(query, cancellationToken);

		// Assert
		await _repository.Received(1).GetAsync(categoryId);
	}
}
