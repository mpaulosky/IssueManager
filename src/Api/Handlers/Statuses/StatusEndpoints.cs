// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     StatusEndpoints.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =============================================

namespace Api.Handlers.Statuses;

/// <summary>Registers Status endpoints on the route builder.</summary>
public static class StatusEndpoints
{
	public static IEndpointRouteBuilder MapStatusEndpoints(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/v1/statuses").WithTags("Statuses");

		group.MapGet("", async (TaxonomyCrudHandler<StatusDto, CreateStatusCommand, UpdateStatusCommand> handler) =>
		{
			var result = await handler.HandleList();
			return Results.Ok(result);
		})
		.WithName("ListStatuses")
		.WithSummary("Get all statuses")
		.Produces<IEnumerable<StatusDto>>(StatusCodes.Status200OK);

		group.MapGet("{id}", async (string id, IStatusRepository repository) =>
		{
			if (!id.TryParseObjectIdOrBadRequest(out var objectId, out var badRequest))
				return badRequest;
			var result = await repository.GetByIdAsync(objectId);
			return result.ToHttpResult(Results.Ok);
		})
		.WithName("GetStatus")
		.WithSummary("Get a status by ID")
		.Produces<StatusDto>(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status404NotFound);

		group.MapPost("", async (CreateStatusCommand command, TaxonomyCrudHandler<StatusDto, CreateStatusCommand, UpdateStatusCommand> handler) =>
		{
			var result = await handler.HandleCreate(command);
			return result.ToHttpResult(value => Results.Created($"/api/v1/statuses/{value.Id}", value));
		})
		.WithName("CreateStatus")
		.WithSummary("Create a new status")
		.Produces<StatusDto>(StatusCodes.Status201Created)
		.Produces(StatusCodes.Status400BadRequest)
		.RequireAuthorization();

		group.MapPatch("{id}", async (string id, UpdateStatusCommand command, TaxonomyCrudHandler<StatusDto, CreateStatusCommand, UpdateStatusCommand> handler) =>
		{
			if (!id.TryParseObjectIdOrBadRequest(out var objectId, out var badRequest))
				return badRequest;
			var commandWithId = command with { Id = objectId };
			var result = await handler.HandleUpdate(commandWithId);
			return result.ToHttpResult(Results.Ok);
		})
		.WithName("UpdateStatus")
		.WithSummary("Update an existing status")
		.Produces<StatusDto>(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status400BadRequest)
		.Produces(StatusCodes.Status404NotFound)
		.RequireAuthorization();

		group.MapDelete("{id}", async (string id, DeleteHandler<StatusDto> handler) =>
		{
			if (!id.TryParseObjectIdOrBadRequest(out var objectId, out var badRequest))
				return badRequest;
			var result = await handler.Handle(objectId);
			return result.ToHttpResult(_ => Results.NoContent());
		})
		.WithName("DeleteStatus")
		.WithSummary("Delete (archive) a status")
		.Produces(StatusCodes.Status204NoContent)
		.Produces(StatusCodes.Status404NotFound)
		.RequireAuthorization();

		return app;
	}
}
