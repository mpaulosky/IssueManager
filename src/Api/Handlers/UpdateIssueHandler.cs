using FluentValidation;
using IssueManager.Api.Data;
using global::Shared.Domain;
using IssueManager.Shared.Validators;
using global::Shared.Exceptions;

namespace IssueManager.Api.Handlers;

/// <summary>
/// Handler for updating existing issues.
/// </summary>
public class UpdateIssueHandler
{
	private readonly IIssueRepository _repository;
	private readonly UpdateIssueValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="UpdateIssueHandler"/> class.
	/// </summary>
	public UpdateIssueHandler(IIssueRepository repository, UpdateIssueValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the update of an existing issue.
	/// </summary>
	public async Task<Issue> Handle(UpdateIssueCommand command, CancellationToken cancellationToken = default)
	{
		// Validate the command
		var validationResult = await _validator.ValidateAsync(command, cancellationToken);
		if (!validationResult.IsValid)
		{
			throw new ValidationException(validationResult.Errors);
		}

		// Get the existing issue
		var existingIssue = await _repository.GetByIdAsync(command.Id, cancellationToken);
		if (existingIssue is null)
		{
			throw new NotFoundException($"Issue with ID '{command.Id}' was not found.");
		}

		// Cannot update an archived issue
		if (existingIssue.IsArchived)
		{
			throw new ConflictException($"Issue with ID '{command.Id}' is archived and cannot be updated.");
		}

		// Update the issue using the domain method
		var updatedIssue = existingIssue.Update(command.Title, command.Description);

		// Persist the updated issue
		var result = await _repository.UpdateAsync(updatedIssue, cancellationToken);
		if (result is null)
		{
			throw new NotFoundException($"Issue with ID '{command.Id}' could not be updated.");
		}
		return result;
	}
}
