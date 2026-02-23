// ============================================
// Copyright (c) 2023. All rights reserved.
// File Name :     CommentMapper.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =============================================

namespace Shared.Mappers;

/// <summary>
///   Extension methods for mapping between <see cref="Shared.Models.Comment"/> and <see cref="CommentDto"/>.
/// </summary>
public static class CommentMapper
{
	/// <summary>
	///   Converts a <see cref="Shared.Models.Comment"/> to a <see cref="CommentDto"/>.
	/// </summary>
	public static CommentDto ToDto(this Shared.Models.Comment comment) =>
		new(
			comment.Id.ToString(),
			comment.Title,
			comment.Description,
			comment.DateCreated,
			comment.Issue,
			comment.Author,
			comment.DateModified);

	/// <summary>
	///   Converts a <see cref="CommentDto"/> to a <see cref="Shared.Models.Comment"/>.
	/// </summary>
	public static Shared.Models.Comment ToModel(this CommentDto dto) =>
		new()
		{
			Title = dto.Title,
			Description = dto.Description,
			Issue = dto.Issue,
			Author = dto.Author,
			DateCreated = dto.DateCreated,
			DateModified = dto.DateModified
		};
}
