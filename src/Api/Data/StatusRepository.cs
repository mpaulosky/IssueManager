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
public class StatusRepository : MongoRepository<Status, StatusDto>, IStatusRepository
{
	/// <summary>
	/// Initializes a new instance of the <see cref="StatusRepository"/> class.
	/// </summary>
	public StatusRepository(string connectionString, string databaseName = "IssueManagerDb")
		: base(connectionString, databaseName, "statuses", "Status")
	{
	}

	/// <inheritdoc />
	protected override StatusDto ToDto(Status model) => model.ToDto();

	/// <inheritdoc />
	protected override Status ToModel(StatusDto dto) => dto.ToModel();

	/// <inheritdoc />
	public async Task<Result<(IReadOnlyList<StatusDto> Items, long Total)>> GetAllAsync(
			int page,
			int pageSize,
			CancellationToken cancellationToken = default)
	{
		var filter = Builders<Status>.Filter.Eq(x => x.Archived, false);
		var total = await Collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
		var entities = await Collection
			.Find(filter)
			.Skip((page - 1) * pageSize)
			.Limit(pageSize)
			.ToListAsync(cancellationToken);

		IReadOnlyList<StatusDto> items = entities.Select(x => x.ToDto()).ToList();
		return Result.Ok((items, total));
	}
}
