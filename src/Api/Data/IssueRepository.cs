// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     IssueRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

using MongoDB.Bson;
using MongoDB.Driver;
using Shared.DTOs;
using Shared.Mappers;
using Shared.Models;

namespace IssueManager.Api.Data;

/// <summary>
/// MongoDB implementation of the issue repository.
/// </summary>
public class IssueRepository : IIssueRepository
{
	private readonly IMongoCollection<Issue> _collection;

	/// <summary>
	/// Initializes a new instance of the <see cref="IssueRepository"/> class.
	/// </summary>
	public IssueRepository(string connectionString, string databaseName = "IssueManagerDb")
	{
		var client = new MongoClient(connectionString);
		var database = client.GetDatabase(databaseName);
		_collection = database.GetCollection<Issue>("issues");
	}

	/// <inheritdoc />
	public async Task<IssueDto> CreateAsync(IssueDto dto, CancellationToken cancellationToken = default)
	{
		var model = dto.ToModel();
		await _collection.InsertOneAsync(model, cancellationToken: cancellationToken);
		return model.ToDto();
	}

	/// <inheritdoc />
	public async Task<IssueDto?> GetByIdAsync(string issueId, CancellationToken cancellationToken = default)
	{
		if (!ObjectId.TryParse(issueId, out var id))
			return null;

		var entity = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
		return entity?.ToDto();
	}

	/// <inheritdoc />
	public async Task<IssueDto?> UpdateAsync(IssueDto dto, CancellationToken cancellationToken = default)
	{
		var model = dto.ToModel();
		var result = await _collection.ReplaceOneAsync(
			x => x.Id == model.Id,
			model,
			cancellationToken: cancellationToken);

		return result.ModifiedCount > 0 ? model.ToDto() : null;
	}

	/// <inheritdoc />
	public async Task<bool> DeleteAsync(string issueId, CancellationToken cancellationToken = default)
	{
		if (!ObjectId.TryParse(issueId, out var id))
			return false;

		var result = await _collection.DeleteOneAsync(x => x.Id == id, cancellationToken);
		return result.DeletedCount > 0;
	}

	/// <inheritdoc />
	public async Task<IReadOnlyList<IssueDto>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		var entities = await _collection.Find(_ => true).ToListAsync(cancellationToken);
		return entities.Select(x => x.ToDto()).ToList().AsReadOnly();
	}

	/// <inheritdoc />
	public async Task<(IReadOnlyList<IssueDto> Items, long Total)> GetAllAsync(
		int page,
		int pageSize,
		CancellationToken cancellationToken = default)
	{
		var filter = Builders<Issue>.Filter.Eq(x => x.Archived, false);
		var total = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
		var entities = await _collection
			.Find(filter)
			.Skip((page - 1) * pageSize)
			.Limit(pageSize)
			.ToListAsync(cancellationToken);

		return (entities.Select(x => x.ToDto()).ToList().AsReadOnly(), total);
	}

	/// <inheritdoc />
	public async Task<bool> ArchiveAsync(string issueId, CancellationToken cancellationToken = default)
	{
		if (!ObjectId.TryParse(issueId, out var id))
			return false;

		var update = Builders<Issue>.Update.Set(x => x.Archived, true);
		var result = await _collection.UpdateOneAsync(x => x.Id == id, update, cancellationToken: cancellationToken);
		return result.ModifiedCount > 0;
	}

	/// <inheritdoc />
	public async Task<long> CountAsync(CancellationToken cancellationToken = default)
	{
		return await _collection.CountDocumentsAsync(_ => true, cancellationToken: cancellationToken);
	}
}
