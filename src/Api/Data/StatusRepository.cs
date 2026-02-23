// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     StatusRepository.cs
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
	public async Task<Result> ArchiveAsync(Status status)
	{
		if (status is null) return Result.Fail("Status cannot be null.");

		status.Archived = true;
		status.DateModified = DateTime.UtcNow;

		var result = await _collection.ReplaceOneAsync(x => x.Id == status.Id, status);
		return result.ModifiedCount > 0
			? Result.Ok()
			: Result.Fail("Status not found or could not be archived.", ResultErrorCode.NotFound);
	}

	/// <inheritdoc />
	public async Task<Result> CreateAsync(Status status)
	{
		if (status is null) return Result.Fail("Status cannot be null.");

		await _collection.InsertOneAsync(status);
		return Result.Ok();
	}

	/// <inheritdoc />
	public async Task<Result<Status>> GetAsync(ObjectId itemId)
	{
		var entity = await _collection.Find(x => x.Id == itemId).FirstOrDefaultAsync();
		return entity is not null
			? Result.Ok(entity)
			: Result.Fail<Status>("Status not found.", ResultErrorCode.NotFound);
	}

	/// <inheritdoc />
	public async Task<Result<IEnumerable<Status>>> GetAllAsync()
	{
		var entities = await _collection.Find(_ => true).ToListAsync();
		return Result.Ok<IEnumerable<Status>>(entities);
	}

	/// <inheritdoc />
	public async Task<Result> UpdateAsync(ObjectId itemId, Status status)
	{
		if (status is null) return Result.Fail("Status cannot be null.");

		status.DateModified = DateTime.UtcNow;

		var result = await _collection.ReplaceOneAsync(x => x.Id == itemId, status);
		return result.ModifiedCount > 0
			? Result.Ok()
			: Result.Fail("Status not found or could not be updated.", ResultErrorCode.NotFound);
	}
}
