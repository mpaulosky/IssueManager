// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DeleteCategoryCommand.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =======================================================

using FluentValidation;

namespace Shared.Validators;

/// <summary>
/// Command for soft-deleting (archiving) a category.
/// </summary>
public record DeleteCategoryCommand
{
	/// <summary>
	/// Gets or sets the category ID.
	/// </summary>
	public ObjectId? Id { get; init; } = string.Empty;
}

/// <summary>
/// Validates the <see cref="DeleteCategoryCommand"/>.
/// </summary>
public class DeleteCategoryValidator : AbstractValidator<DeleteCategoryCommand>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DeleteCategoryValidator"/> class.
	/// </summary>
	public DeleteCategoryValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty()
			.WithMessage("Category ID is required.");
	}
}
