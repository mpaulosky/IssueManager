// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ListCategoriesHandlerIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Integration
// =======================================================

namespace Integration.Handlers;

/// <summary>
/// Integration tests for ListCategoriesHandler with a real MongoDB database.
/// </summary>
[Collection("CategoryIntegration")]
[ExcludeFromCodeCoverage]
public class ListCategoriesHandlerIntegrationTests
{
	private readonly ICategoryRepository _repository;
	private readonly ListCategoriesHandler _handler;

	public ListCategoriesHandlerIntegrationTests(MongoDbFixture fixture)
	{
		_repository = new CategoryRepository(fixture.ConnectionString, $"T{Guid.NewGuid():N}");
		_handler = new ListCategoriesHandler(_repository);
	}

	private static CategoryDto CreateTestCategoryDto(string name, string description = "Test description") =>
		new(ObjectId.GenerateNewId(), name, description, DateTime.UtcNow, null, false, UserDto.Empty);

	[Fact]
	public async Task Handle_EmptyDatabase_ReturnsEmptyList()
	{
		// Arrange - No categories in database

		// Act
		var result = await _handler.Handle(TestContext.Current.CancellationToken);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task Handle_WithCategories_ReturnsAll()
	{
		// Arrange - Create 3 categories
		var category1 = CreateTestCategoryDto("Category 1", "Description 1");
		var category2 = CreateTestCategoryDto("Category 2", "Description 2");
		var category3 = CreateTestCategoryDto("Category 3", "Description 3");

		await _repository.CreateAsync(category1, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(category2, TestContext.Current.CancellationToken);
		await _repository.CreateAsync(category3, TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.Handle(TestContext.Current.CancellationToken);

		// Assert
		IEnumerable<CategoryDto> categoryDtos = result.ToList();
		categoryDtos.Should().HaveCount(3);
		categoryDtos.Should().Contain(c => c.CategoryName == "Category 1");
		categoryDtos.Should().Contain(c => c.CategoryName == "Category 2");
		categoryDtos.Should().Contain(c => c.CategoryName == "Category 3");
	}

	[Fact]
	public async Task Handle_MultipleCategories_ReturnsAll_IncludingArchived()
	{
		// Arrange - Create 5 categories, archive 2
		var categoriesToCreate = new List<CategoryDto>();
		for (int i = 1; i <= 5; i++)
		{
			var category = CreateTestCategoryDto($"Category {i}", $"Description {i}");
			var created = await _repository.CreateAsync(category, TestContext.Current.CancellationToken);
			categoriesToCreate.Add(created.Value!);
		}

		// Archive first 2 categories
		await _repository.ArchiveAsync(categoriesToCreate[0].Id, TestContext.Current.CancellationToken);
		await _repository.ArchiveAsync(categoriesToCreate[1].Id, TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.Handle(TestContext.Current.CancellationToken);

		// Assert
		result.Should().HaveCount(5); // GetAllAsync returns all including archived
	}
}
