using FluentValidation;

namespace Shared.Validators;

/// <summary>
/// Command for creating a new issue.
/// </summary>
public record CreateIssueCommand
{
	/// <summary>
	/// Gets or sets the title of the issue.
	/// </summary>
	public string Title { get; init; } = string.Empty;

	/// <summary>
	/// Gets or sets the description of the issue.
	/// </summary>
	public string? Description { get; init; }

	/// <summary>
	/// Gets or sets the labels to attach to the issue.
	/// </summary>
	public List<string>? Labels { get; init; }
}

/// <summary>
/// Validates the CreateIssueCommand.
/// </summary>
public class CreateIssueValidator : AbstractValidator<CreateIssueCommand>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CreateIssueValidator"/> class.
	/// </summary>
	public CreateIssueValidator()
	{
		RuleFor(x => x.Title)
			.NotEmpty()
			.WithMessage("Title is required.")
			.MinimumLength(3)
			.WithMessage("Title must be at least 3 characters long.")
			.MaximumLength(200)
			.WithMessage("Title cannot exceed 200 characters.");

		RuleFor(x => x.Description)
			.MaximumLength(5000)
			.WithMessage("Description cannot exceed 5000 characters.")
			.When(x => !string.IsNullOrEmpty(x.Description));

		RuleForEach(x => x.Labels)
			.NotEmpty()
			.WithMessage("Label name cannot be empty.")
			.MaximumLength(50)
			.WithMessage("Label name cannot exceed 50 characters.")
			.When(x => x.Labels is not null);
	}
}
