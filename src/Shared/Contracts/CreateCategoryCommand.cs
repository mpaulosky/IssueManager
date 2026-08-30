// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateCategoryCommand.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =======================================================

namespace Shared.Contracts;

/// <summary>
/// Command for creating a new category. Validated by the Api project's generic
/// TaxonomyCommandValidator, configured through CategoryTaxonomyAdapter.
/// </summary>
public record CreateCategoryCommand
{
	/// <summary>
	/// Gets or sets the name of the category.
	/// </summary>
	public string CategoryName { get; init; } = string.Empty;

	/// <summary>
	/// Gets or sets the description of the category.
	/// </summary>
	public string? CategoryDescription { get; init; }
}
