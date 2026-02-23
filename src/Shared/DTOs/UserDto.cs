// ============================================
// Copyright (c) 2023. All rights reserved.
// File Name :     UserDto.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =============================================

namespace Shared.DTOs;

/// <summary>
///   UserDto record
/// </summary>
[Serializable]
public record UserDto(
	string Id,
	string Name,
	string Email,
	DateTime DateCreated = default,
	DateTime? DateModified = null)
{
	/// <summary>
	///   Initializes a new instance of the <see cref="Shared.DTOs.UserDto" /> record.
	/// </summary>
	/// <param name="user">The user.</param>
	public UserDto(User user) : this(user.Id, user.Name, user.Email, user.DateCreated, user.DateModified)
	{
	}

	public static UserDto Empty => new(string.Empty, string.Empty, string.Empty);
}
