// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CategoryEndpoints.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =============================================

using Api.Handlers;

using static Api.Handlers.Categories.GetCategoryHandler;

namespace Api.Handlers.Categories;

/// <summary>Registers Category endpoints on the route builder.</summary>
public static class CategoryEndpoints
{
	public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/v1/categories").WithTags("Categories");

		group.MapGet("", async (ListCategoriesHandler handler) =>
		{
			var result = await handler.Handle();
			return Results.Ok(result);
		})
		.WithName("ListCategories")
		.WithSummary("Get all categories")
		.Produces<IEnumerable<CategoryDto>>(StatusCodes.Status200OK);

		group.MapGet("{id}", async (string id, GetCategoryHandler handler) =>
		{
if (!ObjectId.TryParse(id, out var objectId))
return Results.BadRequest("Invalid ID format");
var query = new GetCategoryQuery(objectId);
			var result = await handler.Handle(query);
			return result.Success ? Results.Ok(result.Value) : Results.NotFound();
		})
		.WithName("GetCategory")
		.WithSummary("Get a category by ID")
		.Produces<CategoryDto>(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status404NotFound);

		group.MapPost("", async (CreateCategoryCommand command, CreateCategoryHandler handler) =>
		{
			var category = await handler.Handle(command);
			return Results.Created($"/api/v1/categories/{category.Id}", category);
		})
		.WithName("CreateCategory")
		.WithSummary("Create a new category")
		.Produces<CategoryDto>(StatusCodes.Status201Created)
		.Produces(StatusCodes.Status400BadRequest)
		.RequireAuthorization();

		group.MapPatch("{id}", async (string id, UpdateCategoryCommand command, UpdateCategoryHandler handler) =>
		{
if (!ObjectId.TryParse(id, out var objectId))
return Results.BadRequest("Invalid ID format");
var commandWithId = command with { Id = objectId };
			var result = await handler.Handle(commandWithId);
			if (!result.Success)
				return result.ErrorCode == ResultErrorCode.NotFound ? Results.NotFound() : Results.BadRequest(result.Error);
			return Results.Ok(result.Value);
		})
		.WithName("UpdateCategory")
		.WithSummary("Update an existing category")
		.Produces<CategoryDto>(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status400BadRequest)
		.Produces(StatusCodes.Status404NotFound)
		.RequireAuthorization();

		group.MapDelete("{id}", async (string id, DeleteCategoryHandler handler) =>
		{
if (!ObjectId.TryParse(id, out var objectId))
return Results.BadRequest("Invalid ID format");
var command = new DeleteCategoryCommand { Id = objectId };
			var result = await handler.Handle(command);
			return result.Success ? Results.NoContent() : Results.NotFound();
		})
		.WithName("DeleteCategory")
		.WithSummary("Delete (archive) a category")
		.Produces(StatusCodes.Status204NoContent)
		.Produces(StatusCodes.Status404NotFound)
		.RequireAuthorization();

		return app;
	}
}
