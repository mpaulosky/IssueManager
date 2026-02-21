// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     Extensions.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Extensions;

/// <summary>
/// Extension methods for mapping between <see cref="Issue"/> and <see cref="IssueDto"/>.
/// </summary>
public static class Extensions
{
	/// <summary>
	/// Creates an <see cref="Issue"/> from a domain <see cref="IssueDto"/>.
	/// </summary>
	/// <param name="issue">The domain issue DTO to convert.</param>
	/// <returns>A new <see cref="Issue"/> populated from <paramref name="issue"/>.</returns>
	public static Issue FromDomain(this IssueDto issue) => new Issue
	{
		Id = issue.Id,
		Title = issue.Title,
		Description = issue.Description,
		DateCreated = issue.DateCreated,
		DateUpdated = issue.DateUpdated,
		Category = issue.Category,
		Author = issue.Author,
		IssueStatus = issue.IssueStatus,
		Archived = issue.Archived,
		ArchivedBy = issue.ArchivedBy ?? UserDto.Empty,
		ApprovedForRelease = issue.ApprovedForRelease,
		Rejected = issue.Rejected,
		Labels = issue.Labels
	};

	/// <summary>
	/// Converts an <see cref="Issue"/> to a domain <see cref="IssueDto"/>.
	/// </summary>
	/// <param name="issue">The entity issue to convert.</param>
	/// <returns>A domain <see cref="IssueDto"/> populated from this entity.</returns>
	public static IssueDto ToDomain(this Issue issue) => new IssueDto(
		issue.Id,
		issue.Title,
		issue.Description,
		issue.DateCreated,
		issue.DateUpdated,
		issue.Category,
		issue.Author,
		issue.IssueStatus,
		issue.Archived,
		issue.ArchivedBy,
		issue.ApprovedForRelease,
		issue.Rejected,
		issue.Labels);
}
