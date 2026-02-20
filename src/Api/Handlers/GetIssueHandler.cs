using IssueManager.Api.Data;

using Shared.Domain;

namespace IssueManager.Api.Handlers;

/// <summary>
/// Query for retrieving a single issue.
/// </summary>
public record GetIssueQuery(string IssueId);

/// <summary>
/// Handler for retrieving issues.
/// </summary>
public class GetIssueHandler
{
	private readonly IIssueRepository _repository;

	/// <summary>
	/// Initializes a new instance of the <see cref="GetIssueHandler"/> class.
	/// </summary>
	public GetIssueHandler(IIssueRepository repository)
	{
		_repository = repository;
	}

	/// <summary>
	/// Handles the retrieval of a single issue.
	/// </summary>
	public async Task<Issue?> Handle(GetIssueQuery query, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(query.IssueId))
		{
			throw new ArgumentException("Issue ID cannot be empty.", nameof(query.IssueId));
		}

		return await _repository.GetByIdAsync(query.IssueId, cancellationToken);
	}

	/// <summary>
	/// Handles the retrieval of all issues.
	/// </summary>
	public async Task<IReadOnlyList<Issue>> HandleGetAll(CancellationToken cancellationToken = default)
	{
		return await _repository.GetAllAsync(cancellationToken);
	}
}
