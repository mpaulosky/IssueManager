// ================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     IIssueRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// ================================================

namespace Api.Data.Interfaces;

/// <summary>
/// Repository interface for issue persistence operations.
/// </summary>
public interface IIssueRepository
{

	/// <summary>
	/// Soft-deletes an issue by setting Archived to true.
	/// </summary>
	Task<Result> ArchiveAsync(ObjectId issueId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new issue in the database.
	/// </summary>
	Task<Result<IssueDto>> CreateAsync(IssueDto issue, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets an issue by its unique identifier.
	/// </summary>
	Task<Result<IssueDto>> GetByIdAsync(ObjectId issueId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets all issues from the database.
	/// </summary>
	Task<Result<IReadOnlyList<IssueDto>>> GetAllAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets paginated issues from the database, excluding archived issues by default.
	/// </summary>
	/// <param name="page">The page number (1-indexed).</param>
	/// <param name="pageSize">The number of items per page.</param>
	/// <param name="searchTerm">Optional search term to filter by title or description.</param>
	/// <param name="authorName">Optional author name to filter by.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task<Result<(IReadOnlyList<IssueDto> Items, long Total)>> GetAllAsync(int page, int pageSize, string? searchTerm = null, string? authorName = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates an existing issue in the database.
	/// </summary>
	Task<Result<IssueDto>> UpdateAsync(IssueDto issue, CancellationToken cancellationToken = default);

	/// <summary>
	/// Counts the total number of issues in the database.
	/// </summary>
	Task<Result<long>> CountAsync(CancellationToken cancellationToken = default);

}
