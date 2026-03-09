// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     IssueRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Data;

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
	public async Task<Result> ArchiveAsync(ObjectId issueId, CancellationToken cancellationToken = default)
	{
		if (issueId == ObjectId.Empty)
			return Result.Fail("Issue cannot be null.");

		var update = Builders<Issue>.Update.Set(x => x.Archived, true);
		var result = await _collection.UpdateOneAsync(x => x.Id == issueId, update, cancellationToken: cancellationToken);
		return result.ModifiedCount > 0 ? Result.Ok() : Result.Fail("Issue not found or already archived.");
	}

	/// <inheritdoc />
	public async Task<Result<IssueDto>> CreateAsync(IssueDto issue, CancellationToken cancellationToken = default)
	{
		if (issue is null)
			return Result.Fail<IssueDto>("Issue cannot be null.");

		var model = issue.ToModel();
		await _collection.InsertOneAsync(model, cancellationToken: cancellationToken);
		return Result.Ok(model.ToDto());
	}

	/// <inheritdoc />
	public async Task<Result<IssueDto>> GetByIdAsync(ObjectId issueId, CancellationToken cancellationToken = default)
	{
		if (!ObjectId.TryParse(issueId.ToString(), out var id))
			return Result.Fail<IssueDto>("Invalid issue ID format.");

		var entity = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);

		return entity is not null ? Result.Ok(entity.ToDto()) : Result.Fail<IssueDto>("Issue not found.");
	}

	/// <inheritdoc />
	public async Task<Result<IReadOnlyList<IssueDto>>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		var entities = await _collection.Find(_ => true).ToListAsync(cancellationToken);
		return Result.Ok<IReadOnlyList<IssueDto>>(entities.Select(x => x.ToDto()).ToList().AsReadOnly());
	}

	/// <inheritdoc />
	public async Task<Result<(IReadOnlyList<IssueDto> Items, long Total)>> GetAllAsync(
			int page,
			int pageSize,
			string? searchTerm = null,
			string? authorName = null,
			CancellationToken cancellationToken = default)
	{
		var filterBuilder = Builders<Issue>.Filter;
		var filters = new List<FilterDefinition<Issue>>
		{
			filterBuilder.Eq(x => x.Archived, false)
		};

		if (!string.IsNullOrWhiteSpace(searchTerm))
		{
			var searchFilter = filterBuilder.Or(
				filterBuilder.Regex(x => x.Title, new BsonRegularExpression(searchTerm, "i")),
				filterBuilder.Regex(x => x.Description, new BsonRegularExpression(searchTerm, "i"))
			);
			filters.Add(searchFilter);
		}

		if (!string.IsNullOrWhiteSpace(authorName))
		{
			filters.Add(filterBuilder.Regex(x => x.Author.Name, new BsonRegularExpression(authorName, "i")));
		}

		var filter = filterBuilder.And(filters);
		var total = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
		var entities = await _collection
			.Find(filter)
			.Skip((page - 1) * pageSize)
			.Limit(pageSize)
			.ToListAsync(cancellationToken);

		IReadOnlyList<IssueDto> items = entities.Select(x => x.ToDto()).ToList();
		return Result.Ok((items, total));
	}

	/// <inheritdoc />
	public async Task<Result<IssueDto>> UpdateAsync(IssueDto dto, CancellationToken cancellationToken = default)
	{
		if (dto is null)
			return Result.Fail<IssueDto>("Issue cannot be null.");

		var model = dto.ToModel();

		var result = await _collection.ReplaceOneAsync(
				x => x.Id == model.Id,
				model,
				cancellationToken: cancellationToken);

		return result.ModifiedCount > 0 ? Result.Ok(model.ToDto()) :
				Result.Fail<IssueDto>("Issue not found or update failed.");
	}

	/// <inheritdoc />
	public async Task<Result<long>> CountAsync(CancellationToken cancellationToken = default)
	{
		return Result.Ok(await _collection.CountDocumentsAsync(_ => true, cancellationToken: cancellationToken));
	}

}
