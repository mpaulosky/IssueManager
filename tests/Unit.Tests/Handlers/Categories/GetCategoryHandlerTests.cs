// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     GetCategoryHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

namespace Unit.Handlers.Categories;

/// <summary>
/// Unit tests for GetCategoryHandler.
/// </summary>
[ExcludeFromCodeCoverage]
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
		var category = new CategoryDto(categoryId, "Bug", "Bug reports", DateTime.UtcNow, null, false, UserDto.Empty);

		_repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Ok(category));

		var query = new GetCategoryQuery(categoryId);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result!.CategoryName.Should().Be("Bug");
		result.CategoryDescription.Should().Be("Bug reports");
		await _repository.Received(1).GetByIdAsync(categoryId, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_NonExistentCategoryId_ReturnsNull()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		_repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Fail("Not found"));

		var query = new GetCategoryQuery(categoryId);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public async Task Handle_EmptyObjectId_ReturnsNull()
	{
		// Arrange
		_repository.GetByIdAsync(ObjectId.Empty, Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Fail("Not found"));
		var query = new GetCategoryQuery(ObjectId.Empty);

		// Act
		var result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeNull();
		await _repository.Received(1).GetByIdAsync(ObjectId.Empty, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_ValidCategoryId_PassesCancellationToken()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var cancellationToken = new CancellationToken();
		var category = new CategoryDto(categoryId, "Feature", "Feature requests", DateTime.UtcNow, null, false, UserDto.Empty);

		_repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
			.Returns(Result<CategoryDto>.Ok(category));

		var query = new GetCategoryQuery(categoryId);

		// Act
		await _handler.Handle(query, cancellationToken);

		// Assert
		await _repository.Received(1).GetByIdAsync(categoryId, Arg.Any<CancellationToken>());
	}
}
