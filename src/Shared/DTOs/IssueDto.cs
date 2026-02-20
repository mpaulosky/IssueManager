// ============================================
// Copyright (c) 2023. All rights reserved.
// File Name :     IssueDto.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =============================================

namespace Shared.DTOs;

/// <summary>
///   IssueDto record
/// </summary>
[Serializable]
public record IssueDto(
	ObjectId Id,
	string Title,
	string Description,
	DateTime DateCreated,
	UserDto Author,
	CategoryDto Category,
	StatusDto Status)
{
	/// <summary>
	///   Initializes a new instance of the <see cref="IssueDto" /> record from a Domain Issue.
	/// </summary>
	/// <param name="issue">The issue.</param>
	public IssueDto(Domain.Issue issue) : this(
		ObjectId.Parse(issue.Id),
		issue.Title,
		issue.Description ?? string.Empty,
		issue.CreatedAt,
		UserDto.Empty,
		CategoryDto.Empty,
		new StatusDto(issue.Status.ToString(), issue.Status.ToString()))
	{
	}

	public static IssueDto Empty => new(
		ObjectId.Empty,
		string.Empty,
		string.Empty,
		DateTime.UtcNow,
		UserDto.Empty,
		CategoryDto.Empty,
		StatusDto.Empty);
}
