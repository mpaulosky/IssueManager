// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ListCategoriesHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

using Api.Data;

using Shared.DTOs;
using Shared.Mappers;

namespace Api.Handlers;

/// <summary>
/// Handler for listing all categories.
/// </summary>
public class ListCategoriesHandler
{
	/// <summary>
	/// The repository for category data access operations.
	/// </summary>
	private readonly ICategoryRepository _repository;

	/// <summary>
	/// Initializes a new instance of the <see cref="ListCategoriesHandler"/> class.
	/// </summary>
	/// <param name="repository">The repository for category data access operations.</param>
	public ListCategoriesHandler(ICategoryRepository repository)
	{
		_repository = repository;
	}

	/// <summary>
	/// Handles the retrieval of all categories.
	/// </summary>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of all categories as <see cref="CategoryDto"/> objects.</returns>
	public async Task<IEnumerable<CategoryDto>> Handle(CancellationToken cancellationToken = default)
	{
		var result = await _repository.GetAllAsync();
		if (result.Failure)
			return Enumerable.Empty<CategoryDto>();

		return result.Value?.Select(c => c.ToDto()) ?? Enumerable.Empty<CategoryDto>();
	}
}
