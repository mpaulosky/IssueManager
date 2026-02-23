// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ListCommentsHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

using Api.Data;

using Shared.DTOs;
using Shared.Mappers;

namespace Api.Handlers;

/// <summary>
/// Handler for listing all comments.
/// </summary>
public class ListCommentsHandler
{
	/// <summary>
	/// The repository for comment data access operations.
	/// </summary>
	private readonly ICommentRepository _repository;

	/// <summary>
	/// Initializes a new instance of the <see cref="ListCommentsHandler"/> class.
	/// </summary>
	/// <param name="repository">The repository for comment data access operations.</param>
	public ListCommentsHandler(ICommentRepository repository)
	{
		_repository = repository;
	}

	/// <summary>
	/// Handles the retrieval of all comments.
	/// </summary>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of all comments as <see cref="CommentDto"/> objects.</returns>
	public async Task<IEnumerable<CommentDto>> Handle(CancellationToken cancellationToken = default)
	{
		var result = await _repository.GetAllAsync();
		if (result.Failure)
			return Enumerable.Empty<CommentDto>();

		return result.Value?.Select(c => c.ToDto()) ?? Enumerable.Empty<CommentDto>();
	}
}
