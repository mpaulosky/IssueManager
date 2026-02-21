using FluentValidation;

namespace IssueManager.Shared.Validators;

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
	}
}
