using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Entities;

using Shared.Domain;

namespace IssueManager.Api.Data;

/// <summary>
/// MongoDB implementation of the issue repository.
/// </summary>
public class IssueRepository : IIssueRepository
{
	private readonly IMongoCollection<IssueEntity> _collection;

	/// <summary>
	/// Initializes a new instance of the <see cref="IssueRepository"/> class.
	/// </summary>
	public IssueRepository(string connectionString, string databaseName = "IssueManagerDb")
	{
		var client = new MongoClient(connectionString);
		var database = client.GetDatabase(databaseName);
		_collection = database.GetCollection<IssueEntity>("issues");
	}

	/// <inheritdoc />
	public async Task<Issue> CreateAsync(Issue issue, CancellationToken cancellationToken = default)
	{
		var entity = IssueEntity.FromDomain(issue);
		await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
		return entity.ToDomain();
	}

	/// <inheritdoc />
	public async Task<Issue?> GetByIdAsync(string issueId, CancellationToken cancellationToken = default)
	{
		var entity = await _collection
			.Find(x => x.Id == issueId)
			.FirstOrDefaultAsync(cancellationToken);

		return entity?.ToDomain();
	}

	/// <inheritdoc />
	public async Task<Issue?> UpdateAsync(Issue issue, CancellationToken cancellationToken = default)
	{
		var entity = IssueEntity.FromDomain(issue);
		var result = await _collection.ReplaceOneAsync(
			x => x.Id == issue.Id,
			entity,
			cancellationToken: cancellationToken);

		return result.ModifiedCount > 0 ? entity.ToDomain() : null;
	}

	/// <inheritdoc />
	public async Task<bool> DeleteAsync(string issueId, CancellationToken cancellationToken = default)
	{
		var result = await _collection.DeleteOneAsync(
			x => x.Id == issueId,
			cancellationToken);

		return result.DeletedCount > 0;
	}

	/// <inheritdoc />
	public async Task<IReadOnlyList<Issue>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		var entities = await _collection
			.Find(_ => true)
			.ToListAsync(cancellationToken);

		return entities.Select(e => e.ToDomain()).ToList();
	}

	/// <inheritdoc />
	public async Task<(IReadOnlyList<Issue> Items, long Total)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
	{
		var filter = Builders<IssueEntity>.Filter.Eq(x => x.IsArchived, false);
		
		var total = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
		
		var entities = await _collection
			.Find(filter)
			.Skip((page - 1) * pageSize)
			.Limit(pageSize)
			.ToListAsync(cancellationToken);

		var items = entities.Select(e => e.ToDomain()).ToList();
		
		return (items, total);
	}

	/// <inheritdoc />
	public async Task<bool> ArchiveAsync(string issueId, string archivedBy, CancellationToken cancellationToken = default)
	{
		var update = Builders<IssueEntity>.Update
			.Set(x => x.IsArchived, true)
			.Set(x => x.ArchivedBy, archivedBy)
			.Set(x => x.ArchivedAt, DateTime.UtcNow)
			.Set(x => x.UpdatedAt, DateTime.UtcNow);
		
		var result = await _collection.UpdateOneAsync(
			x => x.Id == issueId,
			update,
			cancellationToken: cancellationToken);

		return result.ModifiedCount > 0;
	}

	/// <inheritdoc />
	public async Task<long> CountAsync(CancellationToken cancellationToken = default)
	{
		return await _collection.CountDocumentsAsync(_ => true, cancellationToken: cancellationToken);
	}
}

/// <summary>
/// MongoDB entity representation of an issue.
/// </summary>
internal class IssueEntity
{
	[BsonId]
	[BsonRepresentation(BsonType.String)]
	public string Id { get; set; } = string.Empty;

	[BsonElement("title")]
	public string Title { get; set; } = string.Empty;

	[BsonElement("description")]
	public string? Description { get; set; }

	[BsonElement("status")]
	public string Status { get; set; } = string.Empty;

	[BsonElement("createdAt")]
	public DateTime CreatedAt { get; set; }

	[BsonElement("updatedAt")]
	public DateTime UpdatedAt { get; set; }

	[BsonElement("isArchived")]
	public bool IsArchived { get; set; }

	[BsonElement("archivedBy")]
	public string? ArchivedBy { get; set; }

	[BsonElement("archivedAt")]
	public DateTime? ArchivedAt { get; set; }

	[BsonElement("labels")]
	public List<LabelEntity>? Labels { get; set; }

	public static IssueEntity FromDomain(Issue issue)
	{
		return new IssueEntity
		{
			Id = issue.Id,
			Title = issue.Title,
			Description = issue.Description,
			Status = issue.Status.ToString(),
			CreatedAt = issue.CreatedAt,
			UpdatedAt = issue.UpdatedAt,
			IsArchived = issue.IsArchived,
			ArchivedBy = issue.ArchivedBy,
			ArchivedAt = issue.ArchivedAt,
			Labels = issue.Labels?.Select(l => new LabelEntity { Name = l.Name, Color = l.Color }).ToList()
		};
	}

	public Issue ToDomain()
	{
		var issue = new Issue(
			Id: Id,
			Title: Title,
			Description: Description,
			Status: Enum.Parse<IssueStatus>(Status),
			CreatedAt: CreatedAt,
			UpdatedAt: UpdatedAt,
			Labels: Labels?.Select(l => new Label(l.Name, l.Color)).ToList()
		)
		{
			IsArchived = IsArchived,
			ArchivedBy = ArchivedBy,
			ArchivedAt = ArchivedAt
		};
		return issue;
	}
}

/// <summary>
/// MongoDB entity representation of a label.
/// </summary>
internal class LabelEntity
{
	[BsonElement("name")]
	public string Name { get; set; } = string.Empty;

	[BsonElement("color")]
	public string Color { get; set; } = string.Empty;
}
