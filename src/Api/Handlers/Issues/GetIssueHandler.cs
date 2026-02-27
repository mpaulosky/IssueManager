// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     GetIssueHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Handlers.Issues;

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
	public async Task<IssueDto?> Handle(GetIssueQuery query, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(query.IssueId))
			throw new ArgumentException("Issue ID cannot be empty.", nameof(query.IssueId));

		if (!ObjectId.TryParse(query.IssueId, out var issueId))
			return null;

		var result = await _repository.GetByIdAsync(issueId, cancellationToken);
		return result.Success ? result.Value : null;
	}

	/// <summary>
	/// Handles the retrieval of all issues.
	/// </summary>
	public async Task<IReadOnlyList<IssueDto>> HandleGetAll(CancellationToken cancellationToken = default)
	{
		var result = await _repository.GetAllAsync(cancellationToken);
		return result.Value;
	}
}
