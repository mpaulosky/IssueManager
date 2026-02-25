using Api.Handlers;
using Shared.DTOs;
using Shared.Validators;
using static Api.Handlers.GetStatusHandler;

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
			var query = new GetStatusQuery(id);
			var status = await handler.Handle(query);
			return status is not null ? Results.Ok(status) : Results.NotFound();
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
		.Produces(StatusCodes.Status400BadRequest);

		group.MapPatch("{id}", async (string id, UpdateStatusCommand command, UpdateStatusHandler handler) =>
		{
			var commandWithId = command with { Id = id };
			var result = await handler.Handle(commandWithId);
			return result is not null ? Results.Ok(result) : Results.NotFound();
		})
		.WithName("UpdateStatus")
		.WithSummary("Update an existing status")
		.Produces<StatusDto>(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status400BadRequest)
		.Produces(StatusCodes.Status404NotFound);

		group.MapDelete("{id}", async (string id, DeleteStatusHandler handler) =>
		{
			var command = new DeleteStatusCommand { Id = id };
			var result = await handler.Handle(command);
			return result ? Results.NoContent() : Results.NotFound();
		})
		.WithName("DeleteStatus")
		.WithSummary("Delete (archive) a status")
		.Produces(StatusCodes.Status204NoContent)
		.Produces(StatusCodes.Status404NotFound);

		return app;
	}
}
