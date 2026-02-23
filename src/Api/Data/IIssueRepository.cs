// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     IIssueRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

using Shared.DTOs;

namespace IssueManager.Api.Data;

/// <summary>
/// Repository interface for issue persistence operations.
/// </summary>
public interface IIssueRepository
{
	/// <summary>
	/// Creates a new issue in the database.
	/// </summary>
	Task<IssueDto> CreateAsync(IssueDto issue, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets an issue by its unique identifier.
	/// </summary>
	Task<IssueDto?> GetByIdAsync(string issueId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates an existing issue in the database.
	/// </summary>
	Task<IssueDto?> UpdateAsync(IssueDto issue, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes an issue from the database.
	/// </summary>
	Task<bool> DeleteAsync(string issueId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets all issues from the database.
	/// </summary>
	Task<IReadOnlyList<IssueDto>> GetAllAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets paginated issues from the database, excluding archived issues by default.
	/// </summary>
	Task<(IReadOnlyList<IssueDto> Items, long Total)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);

	/// <summary>
	/// Soft-deletes an issue by setting IsArchived to true.
	/// </summary>
	Task<bool> ArchiveAsync(string issueId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Counts the total number of issues in the database.
	/// </summary>
	Task<long> CountAsync(CancellationToken cancellationToken = default);
}
