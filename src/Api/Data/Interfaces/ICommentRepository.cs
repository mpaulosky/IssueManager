// ============================================
// Copyright (c) 2023. All rights reserved.
// File Name :     ICommentRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =============================================
namespace Api.Data.Interfaces;
public interface ICommentRepository : IRepository<CommentDto>
{
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
	/// Registers an upvote for a comment by a specific user.
	/// </summary>
	/// <param name="itemId">The unique identifier of the comment.</param>
	/// <param name="userId"></param>
	/// <param name="cancellationToken"></param>
	/// <returns>A task representing the asynchronous operation.</returns>
	Task<Result> UpVoteAsync(ObjectId itemId, string userId, CancellationToken cancellationToken = default);
}
