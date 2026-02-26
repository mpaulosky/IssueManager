// ============================================
// Copyright (c) 2023. All rights reserved.
// File Name :     StatusDto.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =============================================

namespace Shared.DTOs;

/// <summary>
///   StatusDto record
/// </summary>
[Serializable]
public record StatusDto(
	ObjectId Id,
	string StatusName,
	string StatusDescription,
	DateTime DateCreated,
	DateTime? DateModified,
	bool Archived,
	UserDto ArchivedBy)
{
	/// <summary>
	///   Initializes a new instance of the <see cref="Shared.DTOs.StatusDto" /> record.
	/// </summary>
	/// <param name="status">The status.</param>
	public StatusDto(Status status) : this(
		status.Id,
		status.StatusName,
		status.StatusDescription,
		status.DateCreated,
		status.DateModified,
		status.Archived,
		status.ArchivedBy)
	{
	}

	public static StatusDto Empty => new(
			ObjectId.Empty,
			string.Empty,
			string.Empty,
			DateTime.UtcNow,
			null,
			false,
			UserDto.Empty);
}
