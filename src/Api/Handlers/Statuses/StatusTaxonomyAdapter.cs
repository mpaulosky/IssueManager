// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     StatusTaxonomyAdapter.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Handlers.Statuses;

/// <summary>
/// Bridges <see cref="StatusDto"/>'s StatusName/StatusDescription fields to
/// <see cref="TaxonomyCrudHandler{TDto,TCreateCmd,TUpdateCmd}"/>'s generic name/description shape.
/// </summary>
public static class StatusTaxonomyAdapter
{
	public static readonly TaxonomyAdapter<StatusDto, CreateStatusCommand, UpdateStatusCommand> Instance = new()
	{
		EntityName = "Status",
		GetCreateName = command => command.StatusName,
		GetCreateDescription = command => command.StatusDescription,
		GetUpdateId = command => command.Id,
		GetUpdateName = command => command.StatusName,
		GetUpdateDescription = command => command.StatusDescription,
		NewDto = (id, name, description, createdAt) => new StatusDto(id, name, description, createdAt, null, false, UserDto.Empty),
		WithNameDescription = (dto, name, description) => dto with { StatusName = name, StatusDescription = description }
	};
}
