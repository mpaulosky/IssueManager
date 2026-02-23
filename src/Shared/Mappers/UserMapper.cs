// ============================================
// Copyright (c) 2023. All rights reserved.
// File Name :     UserMapper.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =============================================

namespace Shared.Mappers;

/// <summary>
///   Extension methods for mapping between <see cref="Shared.Models.User"/> and <see cref="UserDto"/>.
/// </summary>
public static class UserMapper
{
	/// <summary>
	///   Converts a <see cref="Shared.Models.User"/> to a <see cref="UserDto"/>.
	/// </summary>
	public static UserDto ToDto(this Shared.Models.User user) =>
		new(user.Id, user.Name, user.Email);

	/// <summary>
	///   Converts a <see cref="UserDto"/> to a <see cref="Shared.Models.User"/>.
	/// </summary>
	public static Shared.Models.User ToModel(this UserDto dto) =>
		new()
		{
			Name = dto.Name,
			Email = dto.Email
		};
}
