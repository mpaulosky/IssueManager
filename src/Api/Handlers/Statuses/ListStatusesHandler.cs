// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ListStatusesHandler.cs
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
/// Handler for listing all statuses.
/// </summary>
public class ListStatusesHandler
{
	private readonly IStatusRepository _repository;

	/// <summary>
	/// Initializes a new instance of the <see cref="ListStatusesHandler"/> class.
	/// </summary>
	public ListStatusesHandler(IStatusRepository repository)
	{
		_repository = repository;
	}

	/// <summary>
	/// Handles the retrieval of all statuses.
	/// </summary>
	public async Task<IEnumerable<StatusDto>> Handle(CancellationToken cancellationToken = default)
	{
		var result = await _repository.GetAllAsync();
		if (result.Failure)
			return Enumerable.Empty<StatusDto>();

		return result.Value?.Select(s => s.ToDto()) ?? Enumerable.Empty<StatusDto>();
	}
}
