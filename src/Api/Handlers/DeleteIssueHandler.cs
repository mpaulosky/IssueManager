using FluentValidation;
using IssueManager.Api.Data;

using Shared.Validators;

namespace IssueManager.Api.Handlers;

/// <summary>
/// Handler for deleting (soft-deleting/archiving) issues.
/// </summary>
public class DeleteIssueHandler
{
	private readonly IIssueRepository _repository;
	private readonly DeleteIssueValidator _validator;
	private readonly IHttpContextAccessor _httpContextAccessor;

	/// <summary>
	/// Initializes a new instance of the <see cref="DeleteIssueHandler"/> class.
	/// </summary>
	public DeleteIssueHandler(IIssueRepository repository, DeleteIssueValidator validator, IHttpContextAccessor httpContextAccessor)
	{
		_repository = repository;
		_validator = validator;
		_httpContextAccessor = httpContextAccessor;
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

		// Get current user from HttpContext
		var currentUser = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "system";

		// Archive the issue (soft-delete)
		return await _repository.ArchiveAsync(command.Id, currentUser, cancellationToken);
	}
}
