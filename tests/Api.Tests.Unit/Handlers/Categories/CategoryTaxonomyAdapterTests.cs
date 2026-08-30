// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CategoryTaxonomyAdapterTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =======================================================

using Api.Data.Interfaces;

namespace Api.Handlers.Categories;

/// <summary>
/// Smoke tests confirming <see cref="CategoryTaxonomyAdapter"/> wires CategoryDto's fields
/// correctly into <see cref="TaxonomyCrudHandler{TDto,TCreateCmd,TUpdateCmd}"/>. Behavior shared
/// by every Taxonomy entity (validation rules, not-found handling, etc.) is covered once by
/// TaxonomyCrudHandlerTests, not repeated here.
/// </summary>
[ExcludeFromCodeCoverage]
public class CategoryTaxonomyAdapterTests
{
	private readonly ICategoryRepository _repository;
	private readonly TaxonomyCrudHandler<CategoryDto, CreateCategoryCommand, UpdateCategoryCommand> _handler;

	public CategoryTaxonomyAdapterTests()
	{
		_repository = Substitute.For<ICategoryRepository>();
		_handler = new TaxonomyCrudHandler<CategoryDto, CreateCategoryCommand, UpdateCategoryCommand>(_repository, CategoryTaxonomyAdapter.Instance);
	}

	[Fact]
	public async Task HandleCreate_ValidCommand_MapsToCategoryNameAndDescription()
	{
		var command = new CreateCategoryCommand { CategoryName = "Bug", CategoryDescription = "Bug reports" };

		_repository.CreateAsync(Arg.Any<CategoryDto>(), Arg.Any<CancellationToken>())
			.Returns(callInfo => Result<CategoryDto>.Ok(callInfo.Arg<CategoryDto>()));

		var result = await _handler.HandleCreate(command, TestContext.Current.CancellationToken);

		result.Success.Should().BeTrue();
		result.Value!.CategoryName.Should().Be("Bug");
		result.Value!.CategoryDescription.Should().Be("Bug reports");
	}

	[Fact]
	public async Task HandleUpdate_ValidCommand_MapsToCategoryNameAndDescription()
	{
		var categoryId = ObjectId.GenerateNewId();
		var existing = new CategoryDto(categoryId, "Old Name", "Old Description", DateTime.UtcNow, null, false, UserDto.Empty);
		var command = new UpdateCategoryCommand { Id = categoryId, CategoryName = "Updated Name", CategoryDescription = "Updated Description" };

		_repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>()).Returns(Result<CategoryDto>.Ok(existing));
		_repository.UpdateAsync(Arg.Any<CategoryDto>(), Arg.Any<CancellationToken>())
			.Returns(callInfo => Result<CategoryDto>.Ok(callInfo.Arg<CategoryDto>()));

		var result = await _handler.HandleUpdate(command, TestContext.Current.CancellationToken);

		result.Success.Should().BeTrue();
		result.Value!.CategoryName.Should().Be("Updated Name");
		result.Value!.CategoryDescription.Should().Be("Updated Description");
	}

	[Fact]
	public async Task HandleList_ReturnsCategoriesFromRepository()
	{
		IReadOnlyList<CategoryDto> categories = new List<CategoryDto>
		{
			new(ObjectId.GenerateNewId(), "Bug", "Bug reports", DateTime.UtcNow, null, false, UserDto.Empty)
		};

		_repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Result<IReadOnlyList<CategoryDto>>.Ok(categories));

		var result = await _handler.HandleList(TestContext.Current.CancellationToken);

		result.Should().ContainSingle(c => c.CategoryName == "Bug");
	}
}
