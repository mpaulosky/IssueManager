using FluentValidation;
using IssueManager.Api.Data;

using Shared.Domain;
using Shared.Validators;

namespace IssueManager.Api.Handlers;

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

		// Create labels if provided
		var labels = command.Labels?
			.Select(l => new Label(l, "#000000"))
			.ToList();

		// Create the issue
		var issue = Issue.Create(
			title: command.Title,
			description: command.Description,
			labels: labels);

		// Persist to database
		return await _repository.CreateAsync(issue, cancellationToken);
	}
}
