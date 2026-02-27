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
	public static IssueDto ToDto(this Issue issue) =>
		new(
			issue.Id,
			issue.Title,
			issue.Description,
			issue.DateCreated,
			issue.DateModified,
			issue.Author,
			issue.Category,
			issue.Status,
			issue.Archived,
			issue.ArchivedBy,
			issue.ApprovedForRelease,
			issue.Rejected);

	/// <summary>
	///   Converts an <see cref="IssueDto"/> to a <see cref="Shared.Models.Issue"/>.
	/// </summary>
	public static Issue ToModel(this IssueDto dto) =>
		new()
		{
			Id = dto.Id,
			Title = dto.Title,
			Description = dto.Description,
			DateCreated = dto.DateCreated,
			DateModified = dto.DateModified,
			Author = dto.Author,
			Category = dto.Category,
			Status = dto.Status,
			Archived = dto.Archived,
			ArchivedBy = dto.ArchivedBy,
			ApprovedForRelease = dto.ApprovedForRelease,
			Rejected = dto.Rejected
		};
}
