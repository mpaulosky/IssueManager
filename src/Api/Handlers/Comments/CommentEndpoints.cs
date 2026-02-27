using Api.Handlers;

using static Api.Handlers.Comments.GetCommentHandler;

namespace Api.Handlers.Comments;

/// <summary>Registers Comment endpoints on the route builder.</summary>
public static class CommentEndpoints
{
	public static IEndpointRouteBuilder MapCommentEndpoints(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/v1/comments").WithTags("Comments");

		group.MapGet("", async (string? issueId, ListCommentsHandler handler) =>
		{
			var result = await handler.Handle(issueId);
			return Results.Ok(result);
		})
		.WithName("ListComments")
		.WithSummary("Get all comments, optionally filtered by issue ID")
		.Produces<IEnumerable<CommentDto>>(StatusCodes.Status200OK);

		group.MapGet("{id}", async (string id, GetCommentHandler handler) =>
		{
			var query = new GetCommentQuery(id);
			var comment = await handler.Handle(query);
			return comment is not null ? Results.Ok(comment) : Results.NotFound();
		})
		.WithName("GetComment")
		.WithSummary("Get a comment by ID")
		.Produces<CommentDto>(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status404NotFound);

		group.MapPost("", async (CreateCommentCommand command, CreateCommentHandler handler) =>
		{
			var comment = await handler.Handle(command);
			return Results.Created($"/api/v1/comments/{comment.Id}", comment);
		})
		.WithName("CreateComment")
		.WithSummary("Create a new comment")
		.Produces<CommentDto>(StatusCodes.Status201Created)
		.Produces(StatusCodes.Status400BadRequest)
		.RequireAuthorization();

		group.MapPatch("{id}", async (string id, UpdateCommentCommand command, UpdateCommentHandler handler) =>
		{
			var commandWithId = command with { Id = id };
			var result = await handler.Handle(commandWithId);
			return result is not null ? Results.Ok(result) : Results.NotFound();
		})
		.WithName("UpdateComment")
		.WithSummary("Update an existing comment")
		.Produces<CommentDto>(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status400BadRequest)
		.Produces(StatusCodes.Status404NotFound)
		.RequireAuthorization();

		group.MapDelete("{id}", async (string id, DeleteCommentHandler handler) =>
		{
			var command = new DeleteCommentCommand { Id = id };
			var result = await handler.Handle(command);
			return result ? Results.NoContent() : Results.NotFound();
		})
		.WithName("DeleteComment")
		.WithSummary("Delete (archive) a comment")
		.Produces(StatusCodes.Status204NoContent)
		.Produces(StatusCodes.Status404NotFound)
		.RequireAuthorization();

		return app;
	}
}
