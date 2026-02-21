using FluentValidation;

namespace IssueManager.Shared.Validators;

/// <summary>
/// Validates the UpdateIssueCommand.
/// </summary>
public class UpdateIssueValidator : AbstractValidator<UpdateIssueCommand>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="UpdateIssueValidator"/> class.
	/// </summary>
	public UpdateIssueValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty()
			.WithMessage("Issue ID is required.");

		RuleFor(x => x.Title)
			.NotEmpty()
			.WithMessage("Title is required.")
			.MinimumLength(3)
			.WithMessage("Title must be at least 3 characters long.")
			.MaximumLength(256)
			.WithMessage("Title cannot exceed 256 characters.");

		RuleFor(x => x.Description)
			.MaximumLength(4096)
			.WithMessage("Description cannot exceed 4096 characters.")
			.When(x => !string.IsNullOrEmpty(x.Description));
	}
}
