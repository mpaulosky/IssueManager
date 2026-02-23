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
	StatusDto Status,
	bool Archived = false,
	UserDto? ArchivedBy = null)
{
	public static IssueDto Empty => new(
		ObjectId.Empty,
		string.Empty,
		string.Empty,
		DateTime.UtcNow,
		UserDto.Empty,
		CategoryDto.Empty,
		StatusDto.Empty);
}
