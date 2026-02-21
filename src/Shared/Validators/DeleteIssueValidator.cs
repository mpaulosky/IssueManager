using FluentValidation;

namespace IssueManager.Shared.Validators;

/// <summary>
/// Validates the DeleteIssueCommand.
/// </summary>
public class DeleteIssueValidator : AbstractValidator<DeleteIssueCommand>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DeleteIssueValidator"/> class.
	/// </summary>
	public DeleteIssueValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty()
			.WithMessage("Issue ID is required.");
	}
}
