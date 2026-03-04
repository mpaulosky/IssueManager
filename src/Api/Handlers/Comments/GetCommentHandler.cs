// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     GetCommentHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Handlers.Comments;

/// <summary>
/// Query for retrieving a single comment.
/// </summary>
/// <param name="CommentId">The unique identifier of the comment to retrieve.</param>
public record GetCommentQuery(ObjectId CommentId);

/// <summary>
/// Handler for retrieving comments.
/// </summary>
public class GetCommentHandler
{
	/// <summary>
	/// The repository for comment data access operations.
	/// </summary>
	private readonly ICommentRepository _repository;

	/// <summary>
	/// Initializes a new instance of the <see cref="GetCommentHandler"/> class.
	/// </summary>
	/// <param name="repository">The repository for comment data access operations.</param>
	public GetCommentHandler(ICommentRepository repository)
	{
		_repository = repository;
	}

	/// <summary>
	/// Handles the retrieval of a single comment by ID.
	/// </summary>
	/// <param name="query">The query containing the comment ID to retrieve.</param>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the comment as a <see cref="CommentDto"/>, or <see langword="null"/> if not found.</returns>
	/// <exception cref="ArgumentException">Thrown when the comment ID is null or empty.</exception>
	public async Task<CommentDto?> Handle(GetCommentQuery query, CancellationToken cancellationToken = default)
	{
		var result = await _repository.GetByIdAsync(query.CommentId, cancellationToken);
		return result.Success ? result.Value : null;
	}
}
