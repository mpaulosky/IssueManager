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
	///   Initializes a new instance of the <see cref="IssueDto" /> record.
	/// </summary>
	/// <param name="issue">The issue.</param>
	public IssueDto(Issue issue) : this(
		issue.Id,
		issue.Title,
		issue.Description,
		issue.DateCreated,
		issue.Author,
		issue.Category,
		issue.IssueStatus)
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
