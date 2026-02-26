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
	public static CommentDto ToDto(this Comment comment) =>
		new(
			comment.Id,
			comment.Title,
			comment.Description,
			comment.DateCreated,
			comment.DateModified,
			comment.Issue,
			comment.Author,
			comment.UserVotes,
			comment.Archived,
			comment.ArchivedBy,
			comment.IsAnswer,
			comment.AnswerSelectedBy


			);

	/// <summary>
	///   Converts a <see cref="CommentDto"/> to a <see cref="Shared.Models.Comment"/>.
	/// </summary>
	public static Comment ToModel(this CommentDto dto) =>
		new()
		{
				Id = dto.Id,
			Title = dto.Title,
			Description = dto.Description,
			DateCreated = dto.DateCreated,
			DateModified = dto.DateModified,
			Issue = dto.Issue,
			Author = dto.Author,
			UserVotes = dto.UserVotes,
			Archived = dto.Archived,
			ArchivedBy = dto.ArchivedBy,
			IsAnswer = dto.IsAnswer,
			AnswerSelectedBy = dto.AnswerSelectedBy
		};
}
