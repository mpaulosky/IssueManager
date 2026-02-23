// ============================================
// Copyright (c) 2023. All rights reserved.
// File Name :     StatusMapper.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =============================================

namespace Shared.Mappers;

/// <summary>
///   Extension methods for mapping between <see cref="Shared.Models.Status"/> and <see cref="StatusDto"/>.
/// </summary>
public static class StatusMapper
{
	/// <summary>
	///   Converts a <see cref="Shared.Models.Status"/> to a <see cref="StatusDto"/>.
	/// </summary>
	public static StatusDto ToDto(this Shared.Models.Status status) =>
		new(status.StatusName, status.StatusDescription, status.Id.ToString());

	/// <summary>
	///   Converts a <see cref="StatusDto"/> to a <see cref="Shared.Models.Status"/>.
	/// </summary>
	public static Shared.Models.Status ToModel(this StatusDto dto) =>
		new()
		{
			StatusName = dto.StatusName,
			StatusDescription = dto.StatusDescription
		};
}
