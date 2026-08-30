// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CategoryTaxonomyAdapter.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Handlers.Categories;

/// <summary>
/// Bridges <see cref="CategoryDto"/>'s CategoryName/CategoryDescription fields to
/// <see cref="TaxonomyCrudHandler{TDto,TCreateCmd,TUpdateCmd}"/>'s generic name/description shape.
/// </summary>
public static class CategoryTaxonomyAdapter
{
	public static readonly TaxonomyAdapter<CategoryDto, CreateCategoryCommand, UpdateCategoryCommand> Instance = new()
	{
		EntityName = "Category",
		GetCreateName = command => command.CategoryName,
		GetCreateDescription = command => command.CategoryDescription,
		GetUpdateId = command => command.Id,
		GetUpdateName = command => command.CategoryName,
		GetUpdateDescription = command => command.CategoryDescription,
		NewDto = (id, name, description, createdAt) => new CategoryDto(id, name, description, createdAt, null, false, UserDto.Empty),
		WithNameDescription = (dto, name, description) => dto with { CategoryName = name, CategoryDescription = description }
	};
}
