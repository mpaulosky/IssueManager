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
	public Task<Result<(IReadOnlyList<StatusDto> Items, long Total)>> GetAllAsync(
			int page,
			int pageSize,
			CancellationToken cancellationToken = default) => GetPagedAsync(page, pageSize, cancellationToken);
}
