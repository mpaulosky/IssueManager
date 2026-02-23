// ============================================
// Copyright (c) 2023. All rights reserved.
// File Name :     IssueMapper.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =============================================

namespace Shared.Mappers;

/// <summary>
///   Extension methods for mapping between <see cref="Shared.Models.Issue"/> and <see cref="IssueDto"/>.
/// </summary>
public static class IssueMapper
{
	/// <summary>
	///   Converts a <see cref="Shared.Models.Issue"/> to an <see cref="IssueDto"/>.
	/// </summary>
	public static IssueDto ToDto(this Shared.Models.Issue issue) =>
		new(
			issue.Id,
			issue.Title,
			issue.Description,
			issue.DateCreated,
			issue.Author,
			issue.Category,
			issue.IssueStatus,
			issue.Archived,
			issue.ArchivedBy,
			issue.DateModified);

	/// <summary>
	///   Converts an <see cref="IssueDto"/> to a <see cref="Shared.Models.Issue"/>.
	/// </summary>
	public static Shared.Models.Issue ToModel(this IssueDto dto) =>
		new()
		{
			Id = dto.Id,
			Title = dto.Title,
			Description = dto.Description,
			DateCreated = dto.DateCreated,
			Author = dto.Author,
			Category = dto.Category,
			IssueStatus = dto.Status,
			Archived = dto.Archived,
			ArchivedBy = dto.ArchivedBy ?? UserDto.Empty,
			DateModified = dto.DateModified
		};
}
