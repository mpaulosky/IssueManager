// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ListStatusesHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Handlers.Statuses;

/// <summary>
/// Handler for listing all statuses.
/// </summary>
public class ListStatusesHandler
{
	/// <summary>
	/// The repository for status data access operations.
	/// </summary>
	private readonly IStatusRepository _repository;

	/// <summary>
	/// Initializes a new instance of the <see cref="ListStatusesHandler"/> class.
	/// </summary>
	/// <param name="repository">The repository for status data access operations.</param>
	public ListStatusesHandler(IStatusRepository repository)
	{
		_repository = repository;
	}

	/// <summary>
	/// Handles the retrieval of all statuses.
	/// </summary>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of all statuses as <see cref="StatusDto"/> objects.</returns>
	public async Task<IEnumerable<StatusDto>> Handle(CancellationToken cancellationToken = default)
	{
		var result = await _repository.GetAllAsync(cancellationToken);
		if (result.Failure)
			return Enumerable.Empty<StatusDto>();

		return result.Value ?? Enumerable.Empty<StatusDto>();
	}
}
