// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ListIssuesQueryValidator.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =======================================================
using FluentValidation;

namespace Shared.Contracts;

/// <summary>
/// Validates the ListIssuesQuery.
/// </summary>
public class ListIssuesQueryValidator : AbstractValidator<ListIssuesQuery>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ListIssuesQueryValidator"/> class.
	/// </summary>
	public ListIssuesQueryValidator()
	{
		RuleFor(x => x.Page)
			.GreaterThanOrEqualTo(1)
			.WithMessage("Page must be greater than or equal to 1.");

		RuleFor(x => x.PageSize)
			.InclusiveBetween(1, 100)
			.WithMessage("Page size must be between 1 and 100.");

		RuleFor(x => x.SearchTerm)
			.MaximumLength(200)
			.When(x => !string.IsNullOrWhiteSpace(x.SearchTerm))
			.WithMessage("Search term must not exceed 200 characters.");

		RuleFor(x => x.AuthorName)
			.MaximumLength(200)
			.When(x => !string.IsNullOrWhiteSpace(x.AuthorName))
			.WithMessage("Author name must not exceed 200 characters.");
	}
}
