// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CategoryRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Abstractions;
using Shared.Models;

namespace Api.Data;

/// <summary>
/// MongoDB implementation of the category repository.
/// </summary>
public class CategoryRepository : ICategoryRepository
{
	private readonly IMongoCollection<Category> _collection;

	/// <summary>
	/// Initializes a new instance of the <see cref="CategoryRepository"/> class.
	/// </summary>
	public CategoryRepository(string connectionString, string databaseName = "IssueManagerDb")
	{
		var client = new MongoClient(connectionString);
		var database = client.GetDatabase(databaseName);
		_collection = database.GetCollection<Category>("categories");
	}

	/// <inheritdoc />
	public async Task<Result> ArchiveAsync(Category category)
	{
		if (category is null) return Result.Fail("Category cannot be null.");

		category.Archived = true;
		category.DateModified = DateTime.UtcNow;

		var result = await _collection.ReplaceOneAsync(x => x.Id == category.Id, category);
		return result.ModifiedCount > 0
			? Result.Ok()
			: Result.Fail("Category not found or could not be archived.", ResultErrorCode.NotFound);
	}

	/// <inheritdoc />
	public async Task<Result> CreateAsync(Category category)
	{
		if (category is null) return Result.Fail("Category cannot be null.");

		await _collection.InsertOneAsync(category);
		return Result.Ok();
	}

	/// <inheritdoc />
	public async Task<Result<Category>> GetAsync(ObjectId itemId)
	{
		var entity = await _collection.Find(x => x.Id == itemId).FirstOrDefaultAsync();
		return entity is not null
			? Result.Ok(entity)
			: Result.Fail<Category>("Category not found.", ResultErrorCode.NotFound);
	}

	/// <inheritdoc />
	public async Task<Result<IEnumerable<Category>>> GetAllAsync()
	{
		var entities = await _collection.Find(_ => true).ToListAsync();
		return Result.Ok<IEnumerable<Category>>(entities);
	}

	/// <inheritdoc />
	public async Task<Result> UpdateAsync(ObjectId itemId, Category category)
	{
		if (category is null) return Result.Fail("Category cannot be null.");

		category.DateModified = DateTime.UtcNow;

		var result = await _collection.ReplaceOneAsync(x => x.Id == itemId, category);
		return result.ModifiedCount > 0
			? Result.Ok()
			: Result.Fail("Category not found or could not be updated.", ResultErrorCode.NotFound);
	}
}
