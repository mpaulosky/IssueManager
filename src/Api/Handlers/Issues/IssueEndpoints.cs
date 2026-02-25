using Api.Handlers;
using Shared.DTOs;
using Shared.Validators;
using static Api.Handlers.GetIssueHandler;

namespace Api.Handlers.Issues;

/// <summary>Registers Issue endpoints on the route builder.</summary>
public static class IssueEndpoints
{
	public static IEndpointRouteBuilder MapIssueEndpoints(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/v1/issues").WithTags("Issues");

		// List Issues (paginated)
		group.MapGet("", async (int? page, int? pageSize, ListIssuesHandler handler) =>
		{
			var query = new ListIssuesQuery { Page = page ?? 1, PageSize = pageSize ?? 20 };
			var result = await handler.Handle(query);
			return Results.Ok(result);
		})
		.WithName("ListIssues")
		.WithSummary("Get a paginated list of issues")
		.Produces<PaginatedResponse<IssueDto>>(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status400BadRequest);

		// Get Issue by ID
		group.MapGet("{id}", async (string id, GetIssueHandler handler) =>
		{
			var query = new GetIssueQuery(id);
			var issue = await handler.Handle(query);
			return issue is not null ? Results.Ok(issue) : Results.NotFound();
		})
		.WithName("GetIssue")
		.WithSummary("Get an issue by ID")
		.Produces(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status404NotFound);

		// Create Issue
		group.MapPost("", async (CreateIssueCommand command, CreateIssueHandler handler) =>
		{
			var issue = await handler.Handle(command);
			return Results.Created($"/api/v1/issues/{issue.Id}", issue);
		})
		.WithName("CreateIssue")
		.WithSummary("Create a new issue")
		.Produces(StatusCodes.Status201Created)
		.Produces(StatusCodes.Status400BadRequest);

		// Update Issue
		group.MapPatch("{id}", async (string id, UpdateIssueCommand command, UpdateIssueHandler handler) =>
		{
			var commandWithId = command with { Id = id };
			var result = await handler.Handle(commandWithId);
			return result is not null ? Results.Ok(result) : Results.NotFound();
		})
		.WithName("UpdateIssue")
		.WithSummary("Update an existing issue")
		.Produces(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status400BadRequest)
		.Produces(StatusCodes.Status404NotFound);

		// Delete Issue (soft-delete)
		group.MapDelete("{id}", async (string id, DeleteIssueHandler handler) =>
		{
			var command = new DeleteIssueCommand { Id = id };
			var result = await handler.Handle(command);
			return result ? Results.NoContent() : Results.NotFound();
		})
		.WithName("DeleteIssue")
		.WithSummary("Delete (archive) an issue")
		.Produces(StatusCodes.Status204NoContent)
		.Produces(StatusCodes.Status404NotFound);

		return app;
	}
}
