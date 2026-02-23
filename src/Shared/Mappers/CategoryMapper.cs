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
	public static CategoryDto ToDto(this Shared.Models.Category category) =>
		new(category.Id, category.CategoryName, category.CategoryDescription);

	/// <summary>
	///   Converts a <see cref="CategoryDto"/> to a <see cref="Shared.Models.Category"/>.
	/// </summary>
	public static Shared.Models.Category ToModel(this CategoryDto dto) =>
		new()
		{
			Id = dto.Id,
			CategoryName = dto.CategoryName,
			CategoryDescription = dto.CategoryDescription
		};
}
