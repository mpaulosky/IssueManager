// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     GetStatusHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Handlers.Statuses;

/// <summary>
/// Query for retrieving a single status.
/// </summary>
/// <param name="StatusId">The unique identifier of the status to retrieve.</param>
public record GetStatusQuery(ObjectId StatusId);

/// <summary>
/// Handler for retrieving statuses.
/// </summary>
public class GetStatusHandler
{
	/// <summary>
	/// The repository for status data access operations.
	/// </summary>
	private readonly IStatusRepository _repository;

	/// <summary>
	/// Initializes a new instance of the <see cref="GetStatusHandler"/> class.
	/// </summary>
	/// <param name="repository">The repository for status data access operations.</param>
	public GetStatusHandler(IStatusRepository repository)
	{
		_repository = repository;
	}

	/// <summary>
	/// Handles the retrieval of a single status by ID.
	/// </summary>
	/// <param name="query">The query containing the status ID to retrieve.</param>
	/// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the status as a <see cref="StatusDto"/>, or <see langword="null"/> if not found.</returns>
	/// <exception cref="ArgumentException">Thrown when the status ID is null or empty.</exception>
	public async Task<StatusDto?> Handle(GetStatusQuery query, CancellationToken cancellationToken = default)
	{
		var result = await _repository.GetByIdAsync(query.StatusId, cancellationToken);
		return result.Success ? result.Value : null;
	}
}
