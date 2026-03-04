// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     GetCategoryHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Handlers.Categories;

/// <summary>
/// Query for retrieving a single category.
/// </summary>
/// <param name="CategoryId">The unique identifier of the category to retrieve.</param>
public record GetCategoryQuery(ObjectId CategoryId);

/// <summary>
/// Handler for retrieving categories.
/// </summary>
public class GetCategoryHandler
{
	/// <summary>
	/// The repository for category data access operations.
	/// </summary>
	private readonly ICategoryRepository _repository;

	/// <summary>
	/// Initializes a new instance of the <see cref="GetCategoryHandler"/> class.
	/// </summary>
	/// <param name="repository">The repository for category data access operations.</param>
	public GetCategoryHandler(ICategoryRepository repository)
	{
		_repository = repository;
	}

	/// <summary>
	/// Handles the retrieval of a single category by ID.
	/// </summary>
	/// <param name="query">The query containing the category ID to retrieve.</param>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the result with category as a <see cref="CategoryDto"/>, or an error if not found.</returns>
	/// <exception cref="ArgumentException">Thrown when the category ID is null or empty.</exception>
	public async Task<Result<CategoryDto>> Handle(GetCategoryQuery query, CancellationToken cancellationToken = default)
	{
		return await _repository.GetByIdAsync(query.CategoryId, cancellationToken);
	}
}
