// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     UpdateCategoryCommand.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =======================================================

using FluentValidation;

namespace Shared.Contracts;

/// <summary>
/// Command for updating an existing category.
/// </summary>
public record UpdateCategoryCommand
{
	/// <summary>
	/// Gets or sets the category ID.
	/// </summary>
	public ObjectId Id { get; init; }

	/// <summary>
	/// Gets or sets the name of the category.
	/// </summary>
	public string CategoryName { get; init; } = string.Empty;

	/// <summary>
	/// Gets or sets the description of the category.
	/// </summary>
	public string? CategoryDescription { get; init; }
}

/// <summary>
/// Validates the <see cref="UpdateCategoryCommand"/>.
/// </summary>
public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="UpdateCategoryValidator"/> class.
	/// </summary>
	public UpdateCategoryValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty()
			.WithMessage("Category ID is required.");

		RuleFor(x => x.CategoryName)
			.NotEmpty()
			.WithMessage("Category name is required.")
			.MinimumLength(2)
			.WithMessage("Category name must be at least 2 characters long.")
			.MaximumLength(100)
			.WithMessage("Category name cannot exceed 100 characters.");

		RuleFor(x => x.CategoryDescription)
			.MaximumLength(500)
			.WithMessage("Category description cannot exceed 500 characters.")
			.When(x => !string.IsNullOrEmpty(x.CategoryDescription));
	}
}
