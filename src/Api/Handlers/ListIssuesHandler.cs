using FluentValidation;
using IssueManager.Api.Data;
using IssueManager.Shared.Domain.DTOs;
using IssueManager.Shared.Validators;

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
	public async Task<PaginatedResponse<IssueResponseDto>> Handle(ListIssuesQuery query, CancellationToken cancellationToken = default)
	{
		// Validate the query
		var validationResult = await _validator.ValidateAsync(query, cancellationToken);
		if (!validationResult.IsValid)
		{
			throw new ValidationException(validationResult.Errors);
		}

		// Get paginated issues from repository
		var (items, total) = await _repository.GetAllAsync(query.Page, query.PageSize, cancellationToken);

		// Convert to DTOs
		var issueDtos = items.Select(issue => new IssueResponseDto
		{
			Id = issue.Id,
			Title = issue.Title,
			Description = issue.Description,
			Status = issue.Status.ToString(),
			CreatedAt = issue.CreatedAt,
			UpdatedAt = issue.UpdatedAt,
			Labels = issue.Labels?.Select(l => l.Name).ToList()
		}).ToList();

		// Calculate total pages
		var totalPages = (int)Math.Ceiling((double)total / query.PageSize);

		return new PaginatedResponse<IssueResponseDto>
		{
			Items = issueDtos,
			Total = total,
			Page = query.Page,
			PageSize = query.PageSize,
			TotalPages = totalPages
		};
	}
}
