// ================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     IIssueRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// ================================================
namespace Api.Data.Interfaces;
public interface IIssueRepository : IRepository<IssueDto>
{
	/// <summary>
	/// Gets paginated issues from the database, excluding archived issues by default.
	/// </summary>
	/// <param name="page">The page number (1-indexed).</param>
	/// <param name="pageSize">The number of items per page.</param>
	/// <param name="searchTerm">Optional search term to filter by title or description.</param>
	/// <param name="authorName">Optional author name to filter by.</param>
	/// <param name="statusName">Optional status name to filter by.</param>
	/// <param name="categoryName">Optional category name to filter by.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task<Result<(IReadOnlyList<IssueDto> Items, long Total)>> GetAllAsync(int page, int pageSize, string? searchTerm = null, string? authorName = null, string? statusName = null, string? categoryName = null, CancellationToken cancellationToken = default);
}
