// ============================================
// Copyright (c) 2023. All rights reserved.
// File Name :     CategoryDto.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =============================================

namespace Shared.DTOs;

/// <summary>
///   CategoryDto record
/// </summary>
[Serializable]
public record CategoryDto(ObjectId Id, string CategoryName, string CategoryDescription)
{
	/// <summary>
	///   Initializes a new instance of the <see cref="CategoryDto" /> record.
	/// </summary>
	/// <param name="category">The category.</param>
	public CategoryDto(Category category) : this(category.Id, category.CategoryName, category.CategoryDescription)
	{
	}

	public static CategoryDto Empty => new(ObjectId.Empty, string.Empty, string.Empty);
}
