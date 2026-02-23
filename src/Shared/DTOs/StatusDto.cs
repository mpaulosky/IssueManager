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
	string StatusName,
	string StatusDescription,
	string? Id = null,
	DateTime DateCreated = default,
	DateTime? DateModified = null)
{
	/// <summary>
	///   Initializes a new instance of the <see cref="Shared.DTOs.StatusDto" /> record.
	/// </summary>
	/// <param name="status">The status.</param>
	public StatusDto(Status status) : this(
		status.StatusName,
		status.StatusDescription,
		status.Id.ToString(),
		status.DateCreated,
		status.DateModified)
	{
	}

	public static StatusDto Empty => new(string.Empty, string.Empty, null);
}
