// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CategoryEndpoints.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =============================================

namespace Api.Handlers.Categories;

/// <summary>Registers Category endpoints on the route builder.</summary>
public static class CategoryEndpoints
{
	public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/v1/categories").WithTags("Categories");

		group.MapGet("", async (TaxonomyCrudHandler<CategoryDto, CreateCategoryCommand, UpdateCategoryCommand> handler) =>
		{
			var result = await handler.HandleList();
			return Results.Ok(result);
		})
		.WithName("ListCategories")
		.WithSummary("Get all categories")
		.Produces<IEnumerable<CategoryDto>>(StatusCodes.Status200OK);

		group.MapGet("{id}", async (string id, ICategoryRepository repository) =>
		{
			if (!id.TryParseObjectIdOrBadRequest(out var objectId, out var badRequest))
				return badRequest;
			var result = await repository.GetByIdAsync(objectId);
			return result.ToHttpResult(Results.Ok);
		})
		.WithName("GetCategory")
		.WithSummary("Get a category by ID")
		.Produces<CategoryDto>(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status404NotFound);

		group.MapPost("", async (CreateCategoryCommand command, TaxonomyCrudHandler<CategoryDto, CreateCategoryCommand, UpdateCategoryCommand> handler) =>
		{
			var result = await handler.HandleCreate(command);
			return result.ToHttpResult(value => Results.Created($"/api/v1/categories/{value.Id}", value));
		})
		.WithName("CreateCategory")
		.WithSummary("Create a new category")
		.Produces<CategoryDto>(StatusCodes.Status201Created)
		.Produces(StatusCodes.Status400BadRequest)
		.RequireAuthorization();

		group.MapPatch("{id}", async (string id, UpdateCategoryCommand command, TaxonomyCrudHandler<CategoryDto, CreateCategoryCommand, UpdateCategoryCommand> handler) =>
		{
			if (!id.TryParseObjectIdOrBadRequest(out var objectId, out var badRequest))
				return badRequest;
			var commandWithId = command with { Id = objectId };
			var result = await handler.HandleUpdate(commandWithId);
			return result.ToHttpResult(Results.Ok);
		})
		.WithName("UpdateCategory")
		.WithSummary("Update an existing category")
		.Produces<CategoryDto>(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status400BadRequest)
		.Produces(StatusCodes.Status404NotFound)
		.RequireAuthorization();

		group.MapDelete("{id}", async (string id, DeleteHandler<CategoryDto> handler) =>
		{
			if (!id.TryParseObjectIdOrBadRequest(out var objectId, out var badRequest))
				return badRequest;
			var result = await handler.Handle(objectId);
			return result.ToHttpResult(_ => Results.NoContent());
		})
		.WithName("DeleteCategory")
		.WithSummary("Delete (archive) a category")
		.Produces(StatusCodes.Status204NoContent)
		.Produces(StatusCodes.Status404NotFound)
		.RequireAuthorization();

		return app;
	}
}
