using FluentValidation;
using IssueManager.Api.Data;
using IssueManager.Shared.Domain;
using IssueManager.Shared.Validators;

namespace IssueManager.Api.Handlers;

/// <summary>
/// Handler for updating issue status.
/// </summary>
public class UpdateIssueStatusHandler
{
	private readonly IIssueRepository _repository;
	private readonly UpdateIssueStatusValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="UpdateIssueStatusHandler"/> class.
	/// </summary>
	public UpdateIssueStatusHandler(IIssueRepository repository, UpdateIssueStatusValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the update of issue status.
	/// </summary>
	public async Task<Issue?> Handle(UpdateIssueStatusCommand command, CancellationToken cancellationToken = default)
	{
		// Validate the command
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
		{
			throw new ValidationException(validationResult.Errors);
		}

		// Retrieve the existing issue
		var existingIssue = await _repository.GetByIdAsync(command.IssueId, cancellationToken);
		if (existingIssue is null)
		{
			return null;
		}

		// Update the status
		var updatedIssue = existingIssue.UpdateStatus(command.Status);

		// Persist changes
		return await _repository.UpdateAsync(updatedIssue, cancellationToken);
	}
}
