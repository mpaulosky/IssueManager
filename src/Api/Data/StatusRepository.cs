// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     StatusRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Data;

/// <summary>
/// MongoDB implementation of the status repository.
/// </summary>
public class StatusRepository : IStatusRepository
{
	private readonly IMongoCollection<Status> _collection;

	/// <summary>
	/// Initializes a new instance of the <see cref="StatusRepository"/> class.
	/// </summary>
	public StatusRepository(string connectionString, string databaseName = "IssueManagerDb")
	{
		var client = new MongoClient(connectionString);
		var database = client.GetDatabase(databaseName);
		_collection = database.GetCollection<Status>("statuses");
	}

	/// <inheritdoc />
	public async Task<Result> ArchiveAsync(ObjectId statusId, CancellationToken cancellationToken = default)
	{
		if (statusId == ObjectId.Empty)
			return Result.Fail("Status cannot be null.");

		var update = Builders<Status>.Update.Set(x => x.Archived, true);
		var result = await _collection.UpdateOneAsync(x => x.Id == statusId, update, cancellationToken: cancellationToken);
		return result.ModifiedCount > 0 ? Result.Ok() : Result.Fail("Status not found or already archived.");
	}

	/// <inheritdoc />
	public async Task<Result<StatusDto>> CreateAsync(StatusDto dto, CancellationToken cancellationToken = default)
	{
		if (dto is null)
			return Result.Fail<StatusDto>("Status cannot be null.");

		var model = dto.ToModel();
		await _collection.InsertOneAsync(model, cancellationToken: cancellationToken);
		return Result.Ok(model.ToDto());
	}

	/// <inheritdoc />
	public async Task<Result<StatusDto>> GetByIdAsync(ObjectId statusId, CancellationToken cancellationToken = default)
	{
		if (!ObjectId.TryParse(statusId.ToString(), out var id))
			return Result.Fail<StatusDto>("Invalid status ID format.");

		var entity = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);

		return entity is not null ? Result.Ok(entity.ToDto()) : Result.Fail<StatusDto>("Status not found.");
	}

	/// <inheritdoc />
	public async Task<Result<IReadOnlyList<StatusDto>>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		var entities = await _collection.Find(_ => true).ToListAsync(cancellationToken);
		return Result.Ok<IReadOnlyList<StatusDto>>(entities.Select(x => x.ToDto()).ToList().AsReadOnly());
	}

	/// <inheritdoc />
	public async Task<Result<(IReadOnlyList<StatusDto> Items, long Total)>> GetAllAsync(
			int page,
			int pageSize,
			CancellationToken cancellationToken = default)
	{
		var filter = Builders<Status>.Filter.Eq(x => x.Archived, false);
		var total = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
		var entities = await _collection
			.Find(filter)
			.Skip((page - 1) * pageSize)
			.Limit(pageSize)
			.ToListAsync(cancellationToken);

		return (Result<(IReadOnlyList<StatusDto> Items, long Total)>)(entities.Count > 0
				? Result.Ok((entities.Select(x => x.ToDto()).ToList().AsReadOnly(), total))
				: Result.Fail("Issues not found."));
	}

	/// <inheritdoc />
	public async Task<Result<StatusDto>> UpdateAsync(StatusDto dto, CancellationToken cancellationToken = default)
	{
		if (dto is null)
			return Result.Fail<StatusDto>("Status cannot be null.");

		var model = dto.ToModel();

		var result = await _collection.ReplaceOneAsync(
				x => x.Id == model.Id,
				model,
				cancellationToken: cancellationToken);

		return result.ModifiedCount > 0 ? Result.Ok(model.ToDto()) :
				Result.Fail<StatusDto>("Status not found or update failed.");
	}

	public async Task<Result<long>> CountAsync(CancellationToken cancellationToken = default)
	{
		return Result.Ok(await _collection.CountDocumentsAsync(_ => true, cancellationToken: cancellationToken));
	}

}
