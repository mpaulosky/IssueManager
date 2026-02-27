// ============================================
// Copyright (c) 2023. All rights reserved.
// File Name :     CategoryMapper.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =============================================

namespace Shared.Mappers;

/// <summary>
///   Extension methods for mapping between <see cref="Shared.Models.Category"/> and <see cref="CategoryDto"/>.
/// </summary>
public static class CategoryMapper
{
	/// <summary>
	///   Converts a <see cref="Shared.Models.Category"/> to a <see cref="CategoryDto"/>.
	/// </summary>
	public static CategoryDto ToDto(this Category category) =>
		new(
			category.Id,
			category.CategoryName,
			category.CategoryDescription,
			category.DateCreated,
			category.DateModified,
			category.Archived,
			category.ArchivedBy);

	/// <summary>
	///   Converts a <see cref="CategoryDto"/> to a <see cref="Shared.Models.Category"/>.
	/// </summary>
	public static Category ToModel(this CategoryDto dto) =>
		new()
		{
			Id = dto.Id,
			CategoryName = dto.CategoryName,
			CategoryDescription = dto.CategoryDescription,
			DateCreated = dto.DateCreated,
			DateModified = dto.DateModified,
			Archived = dto.Archived,
			ArchivedBy = dto.ArchivedBy
		};
}
