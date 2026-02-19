using FluentValidation;
using IssueManager.Shared.Domain;

namespace IssueManager.Shared.Validators;

/// <summary>
/// Command for updating the status of an issue.
/// </summary>
public record UpdateIssueStatusCommand
{
	/// <summary>
	/// Gets or sets the ID of the issue to update.
	/// </summary>
	public string IssueId { get; init; } = string.Empty;

	/// <summary>
	/// Gets or sets the new status for the issue.
	/// </summary>
	public IssueStatus Status { get; init; }
}

/// <summary>
/// Validates the UpdateIssueStatusCommand.
/// </summary>
public class UpdateIssueStatusValidator : AbstractValidator<UpdateIssueStatusCommand>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="UpdateIssueStatusValidator"/> class.
	/// </summary>
	public UpdateIssueStatusValidator()
	{
		RuleFor(x => x.IssueId)
			.NotEmpty()
			.WithMessage("Issue ID is required.");

		RuleFor(x => x.Status)
			.IsInEnum()
			.WithMessage("Status must be a valid IssueStatus value.");
	}
}
