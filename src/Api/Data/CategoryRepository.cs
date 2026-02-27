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
	public async Task<Result> ArchiveAsync(ObjectId categoryId, CancellationToken cancellationToken = default)
	{
		if (categoryId == ObjectId.Empty)
			return Result.Fail("Category cannot be null.");

		var update = Builders<Category>.Update.Set(x => x.Archived, true);
		var result = await _collection.UpdateOneAsync(x => x.Id == categoryId, update, cancellationToken: cancellationToken);
		return result.ModifiedCount > 0 ? Result.Ok() : Result.Fail("Category not found or already archived.");
	}

	/// <inheritdoc />
	public async Task<Result<CategoryDto>> CreateAsync(CategoryDto category, CancellationToken cancellationToken = default)
	{
		if (category is null)
			return Result.Fail<CategoryDto>("Category cannot be null.");

		var model = category.ToModel();
		await _collection.InsertOneAsync(model, cancellationToken: cancellationToken);
		return Result.Ok(model.ToDto());
	}

	/// <inheritdoc />
	public async Task<Result<CategoryDto>> GetByIdAsync(ObjectId categoryId, CancellationToken cancellationToken = default)
	{
		if (!ObjectId.TryParse(categoryId.ToString(), out var id))
			return Result.Fail<CategoryDto>("Invalid category ID format.");

		var entity = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);

		return entity is not null ? Result.Ok(entity.ToDto()) : Result.Fail<CategoryDto>("Category not found.");
	}

	/// <inheritdoc />
	public async Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		var entities = await _collection.Find(_ => true).ToListAsync(cancellationToken);
		return Result.Ok<IReadOnlyList<CategoryDto>>(entities.Select(x => x.ToDto()).ToList().AsReadOnly());
	}

	/// <inheritdoc />
	public async Task<Result<(IReadOnlyList<CategoryDto> Items, long Total)>> GetAllAsync(
			int page,
			int pageSize,
			CancellationToken cancellationToken = default)
	{
		var filter = Builders<Category>.Filter.Eq(x => x.Archived, false);
		var total = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
		var entities = await _collection
			.Find(filter)
			.Skip((page - 1) * pageSize)
			.Limit(pageSize)
			.ToListAsync(cancellationToken);

		return (Result<(IReadOnlyList<CategoryDto> Items, long Total)>)(entities.Count > 0
				? Result.Ok((entities.Select(x => x.ToDto()).ToList().AsReadOnly(), total))
				: Result.Fail("Issues not found."));
	}

	/// <inheritdoc />
	public async Task<Result<CategoryDto>> UpdateAsync(CategoryDto dto, CancellationToken cancellationToken = default)
	{
		if (dto is null)
			return Result.Fail<CategoryDto>("Category cannot be null.");

		var model = dto.ToModel();

		var result = await _collection.ReplaceOneAsync(
				x => x.Id == model.Id,
				model,
				cancellationToken: cancellationToken);

		return result.ModifiedCount > 0 ? Result.Ok(model.ToDto()) :
				Result.Fail<CategoryDto>("Category not found or update failed.");
	}

	/// <inheritdoc />
	public async Task<Result<long>> CountAsync(CancellationToken cancellationToken = default)
	{
		return Result.Ok(await _collection.CountDocumentsAsync(_ => true, cancellationToken: cancellationToken));
	}

}
