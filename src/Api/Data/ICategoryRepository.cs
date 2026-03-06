// ============================================
// Copyright (c) 2023. All rights reserved.
// File Name :     ICategoryRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =============================================

namespace Api.Data;

/// <summary>
/// Provides data access operations for category entities in the data store.
/// </summary>
public interface ICategoryRepository
{

	/// <summary>
	/// Soft-deletes a category by setting Archived to true.
	/// </summary>
	Task<Result> ArchiveAsync(ObjectId categoryId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new category in the database.
	/// </summary>
	Task<Result<CategoryDto>> CreateAsync(CategoryDto category, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets a category by its unique identifier.
	/// </summary>
	Task<Result<CategoryDto>> GetByIdAsync(ObjectId categoryId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets all categories from the database.
	/// </summary>
	Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets paginated categories from the database, excluding archived categories by default.
	/// </summary>
	Task<Result<(IReadOnlyList<CategoryDto> Items, long Total)>> GetAllAsync(
			int page,
			int pageSize,
			CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates an existing category in the database.
	/// </summary>
	Task<Result<CategoryDto>> UpdateAsync(CategoryDto category, CancellationToken cancellationToken = default);

	/// <summary>
	/// Counts the total number of categories in the database.
	/// </summary>
	Task<Result<long>> CountAsync(CancellationToken cancellationToken = default);

}
