using FluentValidation;

using IssueManager.Api.Data;

using Shared.Domain;
using Shared.Validators;

namespace IssueManager.Api.Handlers.Issues;

/// <summary>
/// Handler for creating new issues.
/// </summary>
public class CreateIssueHandler
{
	private readonly IIssueRepository _repository;
	private readonly CreateIssueValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="CreateIssueHandler"/> class.
	/// </summary>
	public CreateIssueHandler(IIssueRepository repository, CreateIssueValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the creation of a new issue.
	/// </summary>
	public async Task<Issue> Handle(CreateIssueCommand command, CancellationToken cancellationToken = default)
	{
		// Validate the command
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
		{
			throw new ValidationException(validationResult.Errors);
		}

		// Create label value objects if provided
		var labels = command.Labels?
			.Select(l => new Label(l, "#000000"))
			.ToList();

		// Build the domain entity and persist
		var issue = Issue.Create(command.Title, command.Description, labels);
		return await _repository.CreateAsync(issue, cancellationToken);
	}
}

