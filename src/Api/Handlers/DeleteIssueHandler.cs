using FluentValidation;
using IssueManager.Api.Data;
using IssueManager.Shared.Validators;
using global::Shared.Exceptions;

namespace IssueManager.Api.Handlers;

/// <summary>
/// Handler for deleting (soft-deleting/archiving) issues.
/// </summary>
public class DeleteIssueHandler
{
	private readonly IIssueRepository _repository;
	private readonly DeleteIssueValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="DeleteIssueHandler"/> class.
	/// </summary>
	public DeleteIssueHandler(IIssueRepository repository, DeleteIssueValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the soft-deletion (archiving) of an issue.
	/// </summary>
	public async Task<bool> Handle(DeleteIssueCommand command, CancellationToken cancellationToken = default)
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

		// If already archived, this is idempotent - return success without updating
		if (existingIssue.IsArchived)
		{
			return true;
		}

		// Archive the issue via the dedicated archive operation
		await _repository.ArchiveAsync(command.Id, cancellationToken);
		return true;
	}
}
