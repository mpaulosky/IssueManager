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
	public static StatusDto ToDto(this Status status) =>
		new(
			status.Id,
			status.StatusName,
			status.StatusDescription,
			status.DateCreated,
			status.DateModified,
			status.Archived,
			status.ArchivedBy);

	/// <summary>
	///   Converts a <see cref="StatusDto"/> to a <see cref="Shared.Models.Status"/>.
	/// </summary>
	public static Status ToModel(this StatusDto dto) =>
		new()
		{
			Id = dto.Id,
			StatusName = dto.StatusName,
			StatusDescription = dto.StatusDescription,
			DateCreated = dto.DateCreated,
			DateModified = dto.DateModified,
			Archived = dto.Archived,
			ArchivedBy = dto.ArchivedBy
		};
}
