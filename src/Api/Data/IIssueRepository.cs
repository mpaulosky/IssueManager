// ================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     IIssueRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// ================================================

namespace Api.Data;

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
	Task<Result<(IReadOnlyList<IssueDto> Items, long Total)>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates an existing issue in the database.
	/// </summary>
	Task<Result<IssueDto>> UpdateAsync(  IssueDto issue, CancellationToken cancellationToken = default);

	/// <summary>
	/// Counts the total number of issues in the database.
	/// </summary>
	Task<Result<long>> CountAsync(CancellationToken cancellationToken = default);

}
