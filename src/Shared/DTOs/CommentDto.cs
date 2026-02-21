// ============================================
// Copyright (c) 2023. All rights reserved.
// File Name :     CommentDto.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =============================================

namespace Shared.DTOs;

/// <summary>
///   CommentDto record
/// </summary>
[Serializable]
public record CommentDto(
	string Id,
	string Title,
	string Description,
	DateTime DateCreated,
	IssueDto Issue,
	UserDto Author)
{
	/// <summary>
	///   Initializes a new instance of the <see cref="CommentDto" /> record.
	/// </summary>
	/// <param name="comment">The comment.</param>
	public CommentDto(Comment comment) : this(
		comment.Id.ToString(),
		comment.Title,
		comment.Description,
		comment.DateCreated,
		comment.Issue,
		comment.Author)
	{
	}

	public static CommentDto Empty => new(
		string.Empty,
		string.Empty,
		string.Empty,
		DateTime.UtcNow,
		IssueDto.Empty,
		UserDto.Empty);
}
