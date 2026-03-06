// ============================================
// Copyright (c) 2023. All rights reserved.
// File Name :     ICommentRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =============================================

namespace Api.Data;

/// <summary>
/// Provides data access operations for comment entities in the data store.
/// </summary>
public interface ICommentRepository
{

	/// <summary>
	/// Soft-deletes a comment by setting Archived to true.
	/// </summary>
	Task<Result> ArchiveAsync(ObjectId commentId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new comment in the database.
	/// </summary>
	Task<Result<CommentDto>> CreateAsync(CommentDto comment, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets a comment by its unique identifier.
	/// </summary>
	Task<Result<CommentDto>> GetByIdAsync(ObjectId commentId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets all comments from the database.
	/// </summary>
	/// <param name="issueId">Optional issue ID to filter comments by specific issue.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task<Result<IReadOnlyList<CommentDto>>> GetAllAsync(string? issueId = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets paginated comments from the database, excluding archived comments by default.
	/// </summary>
	Task<Result<(IReadOnlyList<CommentDto> Items, long Total)>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);

	/// <summary>
	/// Retrieves all comments created by a specific user.
	/// </summary>
	/// <param name="userId"></param>
	/// <param name="cancellationToken"></param>
	/// <returns>
	/// A task that represents the asynchronous operation.
	/// The task result contains an enumerable collection of <see cref="CommentDto"/> instances for the specified user.
	/// </returns>
	Task<Result<IEnumerable<CommentDto>>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Retrieves all comments associated with a specific issue.
	/// </summary>
	/// <param name="issue">The issue to retrieve comments for.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>
	/// A task that represents the asynchronous operation.
	/// The task result contains an enumerable collection of <see cref="CommentDto"/> instances for the specified issue.
	/// </returns>
	Task<Result<IEnumerable<CommentDto>>> GetByIssueAsync(IssueDto issue, CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates an existing comment in the database.
	/// </summary>
	Task<Result<CommentDto>> UpdateAsync(CommentDto comment, CancellationToken cancellationToken = default);

	/// <summary>
	/// Registers an upvote for a comment by a specific user.
	/// </summary>
	/// <param name="itemId">The unique identifier of the comment.</param>
	/// <param name="userId"></param>
	/// <param name="cancellationToken"></param>
	/// <returns>A task representing the asynchronous operation.</returns>
	Task<Result> UpVoteAsync(ObjectId itemId, string userId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Counts the total number of comments in the database.
	/// </summary>
	Task<Result<long>> CountAsync(CancellationToken cancellationToken = default);

}
