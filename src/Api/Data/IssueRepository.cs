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
public class IssueRepository : MongoRepository<Issue, IssueDto>, IIssueRepository
{
	/// <summary>
	/// Initializes a new instance of the <see cref="IssueRepository"/> class.
	/// </summary>
	public IssueRepository(string connectionString, string databaseName = "IssueManagerDb")
		: base(connectionString, databaseName, "issues", "Issue")
	{
	}

	/// <inheritdoc />
	protected override IssueDto ToDto(Issue model) => model.ToDto();

	/// <inheritdoc />
	protected override Issue ToModel(IssueDto dto) => dto.ToModel();

	/// <inheritdoc />
	public async Task<Result<(IReadOnlyList<IssueDto> Items, long Total)>> GetAllAsync(
			int page,
			int pageSize,
			string? searchTerm = null,
			string? authorName = null,
			string? statusName = null,
			string? categoryName = null,
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

		if (!string.IsNullOrWhiteSpace(statusName))
		{
			filters.Add(filterBuilder.Regex(x => x.Status.StatusName, new BsonRegularExpression(statusName, "i")));
		}

		if (!string.IsNullOrWhiteSpace(categoryName))
		{
			filters.Add(filterBuilder.Regex(x => x.Category.CategoryName, new BsonRegularExpression(categoryName, "i")));
		}

		var filter = filterBuilder.And(filters);
		var total = await Collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
		var entities = await Collection
			.Find(filter)
			.Skip((page - 1) * pageSize)
			.Limit(pageSize)
			.ToListAsync(cancellationToken);

		IReadOnlyList<IssueDto> items = entities.Select(x => x.ToDto()).ToList();
		return Result.Ok((items, total));
	}
}
