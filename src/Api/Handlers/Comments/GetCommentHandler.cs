// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     GetCommentHandler.cs
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
/// Query for retrieving a single comment.
/// </summary>
public record GetCommentQuery(string CommentId);

/// <summary>
/// Handler for retrieving comments.
/// </summary>
public class GetCommentHandler
{
	private readonly ICommentRepository _repository;

	/// <summary>
	/// Initializes a new instance of the <see cref="GetCommentHandler"/> class.
	/// </summary>
	public GetCommentHandler(ICommentRepository repository)
	{
		_repository = repository;
	}

	/// <summary>
	/// Handles the retrieval of a single comment by ID.
	/// </summary>
	public async Task<CommentDto?> Handle(GetCommentQuery query, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(query.CommentId))
			throw new ArgumentException("Comment ID cannot be empty.", nameof(query.CommentId));

		if (!MongoDB.Bson.ObjectId.TryParse(query.CommentId, out var objectId))
			return null;

		var result = await _repository.GetAsync(objectId);
		return result.Success ? result.Value?.ToDto() : null;
	}
}
