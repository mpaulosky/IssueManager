// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     GetCategoryHandler.cs
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
/// Query for retrieving a single category.
/// </summary>
public record GetCategoryQuery(string CategoryId);

/// <summary>
/// Handler for retrieving categories.
/// </summary>
public class GetCategoryHandler
{
	private readonly ICategoryRepository _repository;

	/// <summary>
	/// Initializes a new instance of the <see cref="GetCategoryHandler"/> class.
	/// </summary>
	public GetCategoryHandler(ICategoryRepository repository)
	{
		_repository = repository;
	}

	/// <summary>
	/// Handles the retrieval of a single category by ID.
	/// </summary>
	public async Task<CategoryDto?> Handle(GetCategoryQuery query, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(query.CategoryId))
			throw new ArgumentException("Category ID cannot be empty.", nameof(query.CategoryId));

		if (!MongoDB.Bson.ObjectId.TryParse(query.CategoryId, out var objectId))
			return null;

		var result = await _repository.GetAsync(objectId);
		return result.Success ? result.Value?.ToDto() : null;
	}
}
