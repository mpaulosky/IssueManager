// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     MongoRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Data;

/// <summary>
/// Base MongoDB repository implementing the shared CRUD contract (<see cref="IRepository{TDto}"/>)
/// once for any entity. Concrete repositories inherit this for the constructor and the 5 common
/// methods, override <see cref="ToDto"/>/<see cref="ToModel"/> to plug in their entity's mapping,
/// and add only their own distinctive query methods on top.
/// </summary>
public abstract class MongoRepository<TModel, TDto> : IRepository<TDto>
	where TModel : IEntity
{
	/// <summary>
	/// The underlying MongoDB collection for this entity.
	/// </summary>
	protected readonly IMongoCollection<TModel> Collection;

	/// <summary>
	/// The entity name used in default not-found/empty-id error messages (e.g. "Category").
	/// </summary>
	private readonly string _entityName;

	/// <summary>
	/// Initializes the MongoDB client, database, and collection for this entity.
	/// </summary>
	protected MongoRepository(string connectionString, string databaseName, string collectionName, string entityName)
	{
		var client = new MongoClient(connectionString);
		var database = client.GetDatabase(databaseName);
		Collection = database.GetCollection<TModel>(collectionName);
		_entityName = entityName;
	}

	/// <summary>
	/// Converts a persisted model to its DTO. Implemented by calling the entity's existing static mapper.
	/// </summary>
	protected abstract TDto ToDto(TModel model);

	/// <summary>
	/// Converts a DTO to its persisted model. Implemented by calling the entity's existing static mapper.
	/// </summary>
	protected abstract TModel ToModel(TDto dto);

	/// <inheritdoc />
	public async Task<Result> ArchiveAsync(ObjectId id, CancellationToken cancellationToken = default)
	{
		if (id == ObjectId.Empty)
			return Result.Fail($"{_entityName} ID cannot be empty.");

		var update = Builders<TModel>.Update.Set(x => x.Archived, true);
		var result = await Collection.UpdateOneAsync(x => x.Id == id, update, cancellationToken: cancellationToken);
		return result.ModifiedCount > 0
			? Result.Ok()
			: Result.Fail($"{_entityName} not found or already archived.", ResultErrorCode.NotFound);
	}

	/// <inheritdoc />
	public async Task<Result<TDto>> CreateAsync(TDto dto, CancellationToken cancellationToken = default)
	{
		var model = ToModel(dto);
		if (model.Id == ObjectId.Empty)
			return Result.Fail<TDto>($"{_entityName} ID cannot be empty.");

		await Collection.InsertOneAsync(model, cancellationToken: cancellationToken);
		return Result.Ok(ToDto(model));
	}

	/// <inheritdoc />
	public async Task<Result<TDto>> GetByIdAsync(ObjectId id, CancellationToken cancellationToken = default)
	{
		var entity = await Collection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);

		return entity is not null
			? Result.Ok(ToDto(entity))
			: Result.Fail<TDto>($"{_entityName} not found.", ResultErrorCode.NotFound);
	}

	/// <inheritdoc />
	public async Task<Result<IReadOnlyList<TDto>>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		var entities = await Collection.Find(_ => true).ToListAsync(cancellationToken);
		return Result.Ok<IReadOnlyList<TDto>>(entities.Select(ToDto).ToList().AsReadOnly());
	}

	/// <inheritdoc />
	public async Task<Result<TDto>> UpdateAsync(TDto dto, CancellationToken cancellationToken = default)
	{
		var model = ToModel(dto);
		if (model.Id == ObjectId.Empty)
			return Result.Fail<TDto>($"{_entityName} ID cannot be empty.");

		var result = await Collection.ReplaceOneAsync(x => x.Id == model.Id, model, cancellationToken: cancellationToken);

		return result.ModifiedCount > 0
			? Result.Ok(ToDto(model))
			: Result.Fail<TDto>($"{_entityName} not found or update failed.", ResultErrorCode.NotFound);
	}

	/// <inheritdoc />
	public async Task<Result<long>> CountAsync(CancellationToken cancellationToken = default)
	{
		return Result.Ok(await Collection.CountDocumentsAsync(_ => true, cancellationToken: cancellationToken));
	}
}
