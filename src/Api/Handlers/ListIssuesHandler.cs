using FluentValidation;
using IssueManager.Api.Data;
using Shared.DTOs;
using Shared.Validators;

namespace IssueManager.Api.Handlers;

/// <summary>
/// Handler for listing issues with pagination.
/// </summary>
public class ListIssuesHandler
{
	private readonly IIssueRepository _repository;
	private readonly ListIssuesQueryValidator _validator;

	/// <summary>
	/// Initializes a new instance of the <see cref="ListIssuesHandler"/> class.
	/// </summary>
	public ListIssuesHandler(IIssueRepository repository, ListIssuesQueryValidator validator)
	{
		_repository = repository;
		_validator = validator;
	}

	/// <summary>
	/// Handles the retrieval of a paginated list of issues.
	/// </summary>
	public async Task<List<IssueDto>> Handle(ListIssuesQuery query, CancellationToken cancellationToken = default)
	{
		// Validate the query
		var validationResult = await _validator.ValidateAsync(query, cancellationToken);
		if (!validationResult.IsValid)
		{
			throw new ValidationException(validationResult.Errors);
		}

		// Get paginated issues from repository
		var (items, _) = await _repository.GetAllAsync(query.Page, query.PageSize, cancellationToken);

		// Convert to DTOs
		return items.Select(issue => new IssueDto(issue)).ToList();
	}
}
