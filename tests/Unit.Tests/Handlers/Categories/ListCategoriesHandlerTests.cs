// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ListCategoriesHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

using FluentAssertions;
using Api.Data;
using Api.Handlers;
using Shared.Abstractions;
using Shared.Models;
using MongoDB.Bson;
using NSubstitute;

namespace Tests.Unit.Handlers.Categories;

/// <summary>
/// Unit tests for ListCategoriesHandler.
/// </summary>
public class ListCategoriesHandlerTests
{
	private readonly ICategoryRepository _repository;
	private readonly ListCategoriesHandler _handler;

	public ListCategoriesHandlerTests()
	{
		_repository = Substitute.For<ICategoryRepository>();
		_handler = new ListCategoriesHandler(_repository);
	}

	[Fact]
	public async Task Handle_ReturnsAllCategories()
	{
		// Arrange
		IEnumerable<Category> categories = new List<Category>
		{
			new() { Id = ObjectId.GenerateNewId(), CategoryName = "Bug", CategoryDescription = "Bug reports" },
			new() { Id = ObjectId.GenerateNewId(), CategoryName = "Feature", CategoryDescription = "Feature requests" },
			new() { Id = ObjectId.GenerateNewId(), CategoryName = "Enhancement", CategoryDescription = "Enhancements" }
		};

		_repository.GetAllAsync()
			.Returns(Result<IEnumerable<Category>>.Ok(categories));

		// Act
		var result = await _handler.Handle(CancellationToken.None);

		// Assert
		result.Should().HaveCount(3);
		result.Should().Contain(c => c.CategoryName == "Bug");
		result.Should().Contain(c => c.CategoryName == "Feature");
		result.Should().Contain(c => c.CategoryName == "Enhancement");
	}

	[Fact]
	public async Task Handle_NoCategories_ReturnsEmptyList()
	{
		// Arrange
		_repository.GetAllAsync()
			.Returns(Result<IEnumerable<Category>>.Ok((IEnumerable<Category>)new List<Category>()));

		// Act
		var result = await _handler.Handle(CancellationToken.None);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_RepositoryFails_ReturnsEmptyList()
	{
		// Arrange
		_repository.GetAllAsync()
			.Returns(Result<IEnumerable<Category>>.Fail("Database error"));

		// Act
		var result = await _handler.Handle(CancellationToken.None);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_RepositoryReturnsNull_ReturnsEmptyList()
	{
		// Arrange
		_repository.GetAllAsync()
			.Returns(Result<IEnumerable<Category>>.Ok((IEnumerable<Category>?)null!));

		// Act
		var result = await _handler.Handle(CancellationToken.None);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_PassesCancellationToken()
	{
		// Arrange
		var cancellationToken = new CancellationToken();
		IEnumerable<Category> categories = new List<Category>
		{
			new() { Id = ObjectId.GenerateNewId(), CategoryName = "Test" }
		};

		_repository.GetAllAsync()
			.Returns(Result<IEnumerable<Category>>.Ok(categories));

		// Act
		await _handler.Handle(cancellationToken);

		// Assert
		await _repository.Received(1).GetAllAsync();
	}
}
