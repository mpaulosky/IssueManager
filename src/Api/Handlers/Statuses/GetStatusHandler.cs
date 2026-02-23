// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     GetStatusHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

using Api.Data;
using Shared.DTOs;
using Shared.Mappers;

namespace Api.Handlers;

/// <summary>
/// Query for retrieving a single status.
/// </summary>
public record GetStatusQuery(string StatusId);

/// <summary>
/// Handler for retrieving statuses.
/// </summary>
public class GetStatusHandler
{
	private readonly IStatusRepository _repository;

	/// <summary>
	/// Initializes a new instance of the <see cref="GetStatusHandler"/> class.
	/// </summary>
	public GetStatusHandler(IStatusRepository repository)
	{
		_repository = repository;
	}

	/// <summary>
	/// Handles the retrieval of a single status by ID.
	/// </summary>
	public async Task<StatusDto?> Handle(GetStatusQuery query, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(query.StatusId))
			throw new ArgumentException("Status ID cannot be empty.", nameof(query.StatusId));

		if (!MongoDB.Bson.ObjectId.TryParse(query.StatusId, out var objectId))
			return null;

		var result = await _repository.GetAsync(objectId);
		return result.Success ? result.Value?.ToDto() : null;
	}
}
