// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     StatusEndpoints.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =============================================

using Api.Handlers;

using static Api.Handlers.Statuses.GetStatusHandler;

namespace Api.Handlers.Statuses;

/// <summary>Registers Status endpoints on the route builder.</summary>
public static class StatusEndpoints
{
	public static IEndpointRouteBuilder MapStatusEndpoints(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/v1/statuses").WithTags("Statuses");

		group.MapGet("", async (ListStatusesHandler handler) =>
		{
			var result = await handler.Handle();
			return Results.Ok(result);
		})
		.WithName("ListStatuses")
		.WithSummary("Get all statuses")
		.Produces<IEnumerable<StatusDto>>(StatusCodes.Status200OK);

		group.MapGet("{id}", async (string id, GetStatusHandler handler) =>
		{
if (!ObjectId.TryParse(id, out var objectId))
return Results.BadRequest("Invalid ID format");
var query = new GetStatusQuery(objectId);
			var result = await handler.Handle(query);
			return result.Success ? Results.Ok(result.Value) : Results.NotFound();
		})
		.WithName("GetStatus")
		.WithSummary("Get a status by ID")
		.Produces<StatusDto>(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status404NotFound);

		group.MapPost("", async (CreateStatusCommand command, CreateStatusHandler handler) =>
		{
			var status = await handler.Handle(command);
			return Results.Created($"/api/v1/statuses/{status.Id}", status);
		})
		.WithName("CreateStatus")
		.WithSummary("Create a new status")
		.Produces<StatusDto>(StatusCodes.Status201Created)
		.Produces(StatusCodes.Status400BadRequest)
		.RequireAuthorization();

		group.MapPatch("{id}", async (string id, UpdateStatusCommand command, UpdateStatusHandler handler) =>
		{
if (!ObjectId.TryParse(id, out var objectId))
return Results.BadRequest("Invalid ID format");
var commandWithId = command with { Id = objectId };
			var result = await handler.Handle(commandWithId);
			if (!result.Success)
				return result.ErrorCode == ResultErrorCode.NotFound ? Results.NotFound() : Results.BadRequest(result.Error);
			return Results.Ok(result.Value);
		})
		.WithName("UpdateStatus")
		.WithSummary("Update an existing status")
		.Produces<StatusDto>(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status400BadRequest)
		.Produces(StatusCodes.Status404NotFound)
		.RequireAuthorization();

		group.MapDelete("{id}", async (string id, DeleteStatusHandler handler) =>
		{
if (!ObjectId.TryParse(id, out var objectId))
return Results.BadRequest("Invalid ID format");
var command = new DeleteStatusCommand { Id = objectId };
			var result = await handler.Handle(command);
			return result.Success ? Results.NoContent() : Results.NotFound();
		})
		.WithName("DeleteStatus")
		.WithSummary("Delete (archive) a status")
		.Produces(StatusCodes.Status204NoContent)
		.Produces(StatusCodes.Status404NotFound)
		.RequireAuthorization();

		return app;
	}
}
