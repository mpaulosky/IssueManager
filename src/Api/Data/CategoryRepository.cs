// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CategoryRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Data;

/// <summary>
/// MongoDB implementation of the category repository.
/// </summary>
public class CategoryRepository : MongoRepository<Category, CategoryDto>, ICategoryRepository
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CategoryRepository"/> class.
	/// </summary>
	public CategoryRepository(string connectionString, string databaseName = "IssueManagerDb")
		: base(connectionString, databaseName, "categories", "Category")
	{
	}

	/// <inheritdoc />
	protected override CategoryDto ToDto(Category model) => model.ToDto();

	/// <inheritdoc />
	protected override Category ToModel(CategoryDto dto) => dto.ToModel();

	/// <inheritdoc />
	public Task<Result<(IReadOnlyList<CategoryDto> Items, long Total)>> GetAllAsync(
			int page,
			int pageSize,
			CancellationToken cancellationToken = default) => GetPagedAsync(page, pageSize, cancellationToken);
}
