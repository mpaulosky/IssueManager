// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ListCategoriesHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =======================================================

namespace Unit.Handlers.Categories;

/// <summary>
/// Unit tests for ListCategoriesHandler.
/// </summary>
[ExcludeFromCodeCoverage]
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
		IReadOnlyList<CategoryDto> categories = new List<CategoryDto>
		{
			new(ObjectId.GenerateNewId(), "Bug", "Bug reports", DateTime.UtcNow, null, false, UserDto.Empty),
			new(ObjectId.GenerateNewId(), "Feature", "Feature requests", DateTime.UtcNow, null, false, UserDto.Empty),
			new(ObjectId.GenerateNewId(), "Enhancement", "Enhancements", DateTime.UtcNow, null, false, UserDto.Empty)
		};

		_repository.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Result<IReadOnlyList<CategoryDto>>.Ok(categories));

		// Act
		var result = await _handler.Handle(TestContext.Current.CancellationToken);

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
		_repository.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Result<IReadOnlyList<CategoryDto>>.Ok((IReadOnlyList<CategoryDto>)new List<CategoryDto>()));

		// Act
		var result = await _handler.Handle(TestContext.Current.CancellationToken);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_RepositoryFails_ReturnsEmptyList()
	{
		// Arrange
		_repository.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Result<IReadOnlyList<CategoryDto>>.Fail("Database error"));

		// Act
		var result = await _handler.Handle(TestContext.Current.CancellationToken);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_RepositoryReturnsNull_ReturnsEmptyList()
	{
		// Arrange
		_repository.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Result<IReadOnlyList<CategoryDto>>.Ok((IReadOnlyList<CategoryDto>)null!));

		// Act
		var result = await _handler.Handle(TestContext.Current.CancellationToken);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_PassesCancellationToken()
	{
		// Arrange
		IReadOnlyList<CategoryDto> categories = new List<CategoryDto>
		{
			new(ObjectId.GenerateNewId(), "Test", "", DateTime.UtcNow, null, false, UserDto.Empty)
		};

		_repository.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Result<IReadOnlyList<CategoryDto>>.Ok(categories));

		// Act
		await _handler.Handle(TestContext.Current.CancellationToken);

		// Assert
		await _repository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
	}
}
