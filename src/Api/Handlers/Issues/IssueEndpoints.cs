// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     IssueEndpoints.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =============================================

using Api.Handlers;

using static Api.Handlers.Issues.GetIssueHandler;

namespace Api.Handlers.Issues;

/// <summary>Registers Issue endpoints on the route builder.</summary>
public static class IssueEndpoints
{
	public static IEndpointRouteBuilder MapIssueEndpoints(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/v1/issues").WithTags("Issues");

		// List Issues (paginated)
		group.MapGet("", async (int? page, int? pageSize, string? searchTerm, string? authorName, ListIssuesHandler handler) =>
		{
			var query = new ListIssuesQuery
			{
				Page = page ?? 1,
				PageSize = pageSize ?? 20,
				SearchTerm = searchTerm,
				AuthorName = authorName
			};
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
			if (!ObjectId.TryParse(id, out var objectId))
				return Results.BadRequest("Invalid ID format");
			var query = new GetIssueQuery(objectId);
			var result = await handler.Handle(query);
			return result.Success ? Results.Ok(result.Value) : Results.NotFound();
		})
		.WithName("GetIssue")
		.WithSummary("Get an issue by ID")
		.Produces(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status404NotFound);

		// Create Issue
		group.MapPost("", async (CreateIssueCommand command, CreateIssueHandler handler) =>
		{
			var result = await handler.Handle(command);
			if (!result.Success)
				return result.ErrorCode == ResultErrorCode.Validation
					? Results.BadRequest(result.Error)
					: Results.BadRequest(result.Error);
			return Results.Created($"/api/v1/issues/{result.Value!.Id}", result.Value);
		})
		.WithName("CreateIssue")
		.WithSummary("Create a new issue")
		.Produces(StatusCodes.Status201Created)
		.Produces(StatusCodes.Status400BadRequest)
		.RequireAuthorization();

		// Update Issue
		group.MapPatch("{id}", async (string id, UpdateIssueCommand command, UpdateIssueHandler handler) =>
		{
			if (!ObjectId.TryParse(id, out var objectId))
				return Results.BadRequest("Invalid ID format");
			var commandWithId = command with { Id = objectId };
			var result = await handler.Handle(commandWithId);
			if (!result.Success)
				return result.ErrorCode == ResultErrorCode.NotFound ? Results.NotFound()
					: result.ErrorCode == ResultErrorCode.Conflict ? Results.Conflict(result.Error)
					: Results.BadRequest(result.Error);
			return Results.Ok(result.Value);
		})
		.WithName("UpdateIssue")
		.WithSummary("Update an existing issue")
		.Produces(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status400BadRequest)
		.Produces(StatusCodes.Status404NotFound)
		.RequireAuthorization();

		// Update Issue Status (Admin only)
		group.MapPatch("{id}/status", async (string id, UpdateIssueStatusCommand command, UpdateIssueStatusHandler handler) =>
		{
			if (!ObjectId.TryParse(id, out var objectId))
				return Results.BadRequest("Invalid ID format");
			var commandWithId = command with { IssueId = objectId };
			var result = await handler.Handle(commandWithId);
			if (!result.Success)
				return result.ErrorCode == ResultErrorCode.NotFound ? Results.NotFound()
					: Results.BadRequest(result.Error);
			return Results.Ok(result.Value);
		})
		.WithName("UpdateIssueStatus")
		.WithSummary("Update the status of an issue")
		.Produces(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status400BadRequest)
		.Produces(StatusCodes.Status404NotFound)
		.RequireAuthorization("Admin");

		// Delete Issue (soft-delete)
		group.MapDelete("{id}", async (string id, DeleteIssueHandler handler) =>
		{
			if (!ObjectId.TryParse(id, out var objectId))
				return Results.BadRequest("Invalid ID format");
			var command = new DeleteIssueCommand { Id = objectId };
			var result = await handler.Handle(command);
			return result.Success ? Results.NoContent() : Results.NotFound();
		})
		.WithName("DeleteIssue")
		.WithSummary("Delete (archive) an issue")
		.Produces(StatusCodes.Status204NoContent)
		.Produces(StatusCodes.Status404NotFound)
		.RequireAuthorization();

		return app;
	}
}
