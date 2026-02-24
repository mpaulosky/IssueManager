using ServiceDefaults;
using Api.Data;
using Api.Handlers;
using Shared.Validators;
using Shared.DTOs;
using static Api.Handlers.GetIssueHandler;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceDefaults();

builder.Services.AddOpenApi();

// Register repository
var connectionString = builder.Configuration.GetConnectionString("issuemanager") 
	?? "mongodb://localhost:27017";
builder.Services.AddSingleton<IIssueRepository>(sp => 
	new IssueRepository(connectionString, "IssueManagerDb"));
builder.Services.AddSingleton<IStatusRepository>(sp =>
	new StatusRepository(connectionString, "IssueManagerDb"));
builder.Services.AddSingleton<ICategoryRepository>(sp =>
	new CategoryRepository(connectionString, "IssueManagerDb"));
builder.Services.AddSingleton<ICommentRepository>(sp =>
	new CommentRepository(connectionString, "IssueManagerDb"));

// Register validators
builder.Services.AddSingleton<CreateIssueValidator>();
builder.Services.AddSingleton<UpdateIssueValidator>();
builder.Services.AddSingleton<DeleteIssueValidator>();
builder.Services.AddSingleton<ListIssuesQueryValidator>();
builder.Services.AddSingleton<UpdateIssueStatusValidator>();
builder.Services.AddSingleton<CreateStatusValidator>();
builder.Services.AddSingleton<UpdateStatusValidator>();
builder.Services.AddSingleton<DeleteStatusValidator>();
builder.Services.AddSingleton<CreateCategoryValidator>();
builder.Services.AddSingleton<UpdateCategoryValidator>();
builder.Services.AddSingleton<DeleteCategoryValidator>();
builder.Services.AddSingleton<CreateCommentValidator>();
builder.Services.AddSingleton<UpdateCommentValidator>();
builder.Services.AddSingleton<DeleteCommentValidator>();

// Register handlers
builder.Services.AddSingleton<CreateIssueHandler>();
builder.Services.AddSingleton<UpdateIssueHandler>();
builder.Services.AddSingleton<DeleteIssueHandler>();
builder.Services.AddSingleton<ListIssuesHandler>();
builder.Services.AddSingleton<GetIssueHandler>();
builder.Services.AddSingleton<UpdateIssueStatusHandler>();
builder.Services.AddSingleton<CreateStatusHandler>();
builder.Services.AddSingleton<GetStatusHandler>();
builder.Services.AddSingleton<ListStatusesHandler>();
builder.Services.AddSingleton<UpdateStatusHandler>();
builder.Services.AddSingleton<DeleteStatusHandler>();
builder.Services.AddSingleton<CreateCategoryHandler>();
builder.Services.AddSingleton<GetCategoryHandler>();
builder.Services.AddSingleton<ListCategoriesHandler>();
builder.Services.AddSingleton<UpdateCategoryHandler>();
builder.Services.AddSingleton<DeleteCategoryHandler>();
builder.Services.AddSingleton<CreateCommentHandler>();
builder.Services.AddSingleton<GetCommentHandler>();
builder.Services.AddSingleton<ListCommentsHandler>();
builder.Services.AddSingleton<UpdateCommentHandler>();
builder.Services.AddSingleton<DeleteCommentHandler>();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapOpenApi();

// Issue API Endpoints
var issuesApi = app.MapGroup("/api/v1/issues")
	.WithTags("Issues");

// List Issues (paginated)
issuesApi.MapGet("", async (int? page, int? pageSize, ListIssuesHandler handler) =>
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
issuesApi.MapGet("{id}", async (string id, GetIssueHandler handler) =>
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
issuesApi.MapPost("", async (CreateIssueCommand command, CreateIssueHandler handler) =>
{
	var issue = await handler.Handle(command);
	return Results.Created($"/api/v1/issues/{issue.Id}", issue);
})
.WithName("CreateIssue")
.WithSummary("Create a new issue")
.Produces(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest);

// Update Issue
issuesApi.MapPatch("{id}", async (string id, UpdateIssueCommand command, UpdateIssueHandler handler) =>
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
issuesApi.MapDelete("{id}", async (string id, DeleteIssueHandler handler) =>
{
	var command = new DeleteIssueCommand { Id = id };
	var result = await handler.Handle(command);
	return result ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteIssue")
.WithSummary("Delete (archive) an issue")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

app.MapHealthChecks("/health");

// Status API Endpoints
var statusesApi = app.MapGroup("/api/v1/statuses").WithTags("Statuses");

statusesApi.MapGet("", async (ListStatusesHandler handler) =>
{
	var result = await handler.Handle();
	return Results.Ok(result);
})
.WithName("ListStatuses")
.WithSummary("Get all statuses")
.Produces<IEnumerable<StatusDto>>(StatusCodes.Status200OK);

statusesApi.MapGet("{id}", async (string id, GetStatusHandler handler) =>
{
	var query = new GetStatusQuery(id);
	var status = await handler.Handle(query);
	return status is not null ? Results.Ok(status) : Results.NotFound();
})
.WithName("GetStatus")
.WithSummary("Get a status by ID")
.Produces<StatusDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

statusesApi.MapPost("", async (CreateStatusCommand command, CreateStatusHandler handler) =>
{
	var status = await handler.Handle(command);
	return Results.Created($"/api/v1/statuses/{status.Id}", status);
})
.WithName("CreateStatus")
.WithSummary("Create a new status")
.Produces<StatusDto>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest);

statusesApi.MapPatch("{id}", async (string id, UpdateStatusCommand command, UpdateStatusHandler handler) =>
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

statusesApi.MapDelete("{id}", async (string id, DeleteStatusHandler handler) =>
{
	var command = new DeleteStatusCommand { Id = id };
	var result = await handler.Handle(command);
	return result ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteStatus")
.WithSummary("Delete (archive) a status")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

// Category API Endpoints
var categoriesApi = app.MapGroup("/api/v1/categories").WithTags("Categories");

categoriesApi.MapGet("", async (ListCategoriesHandler handler) =>
{
	var result = await handler.Handle();
	return Results.Ok(result);
})
.WithName("ListCategories")
.WithSummary("Get all categories")
.Produces<IEnumerable<CategoryDto>>(StatusCodes.Status200OK);

categoriesApi.MapGet("{id}", async (string id, GetCategoryHandler handler) =>
{
	var query = new GetCategoryQuery(id);
	var category = await handler.Handle(query);
	return category is not null ? Results.Ok(category) : Results.NotFound();
})
.WithName("GetCategory")
.WithSummary("Get a category by ID")
.Produces<CategoryDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

categoriesApi.MapPost("", async (CreateCategoryCommand command, CreateCategoryHandler handler) =>
{
	var category = await handler.Handle(command);
	return Results.Created($"/api/v1/categories/{category.Id}", category);
})
.WithName("CreateCategory")
.WithSummary("Create a new category")
.Produces<CategoryDto>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest);

categoriesApi.MapPatch("{id}", async (string id, UpdateCategoryCommand command, UpdateCategoryHandler handler) =>
{
	var commandWithId = command with { Id = id };
	var result = await handler.Handle(commandWithId);
	return result is not null ? Results.Ok(result) : Results.NotFound();
})
.WithName("UpdateCategory")
.WithSummary("Update an existing category")
.Produces<CategoryDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status404NotFound);

categoriesApi.MapDelete("{id}", async (string id, DeleteCategoryHandler handler) =>
{
	var command = new DeleteCategoryCommand { Id = id };
	var result = await handler.Handle(command);
	return result ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteCategory")
.WithSummary("Delete (archive) a category")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

// Comment API Endpoints
var commentsApi = app.MapGroup("/api/v1/comments").WithTags("Comments");

commentsApi.MapGet("", async (ListCommentsHandler handler) =>
{
	var result = await handler.Handle();
	return Results.Ok(result);
})
.WithName("ListComments")
.WithSummary("Get all comments")
.Produces<IEnumerable<CommentDto>>(StatusCodes.Status200OK);

commentsApi.MapGet("{id}", async (string id, GetCommentHandler handler) =>
{
	var query = new GetCommentQuery(id);
	var comment = await handler.Handle(query);
	return comment is not null ? Results.Ok(comment) : Results.NotFound();
})
.WithName("GetComment")
.WithSummary("Get a comment by ID")
.Produces<CommentDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

commentsApi.MapPost("", async (CreateCommentCommand command, CreateCommentHandler handler) =>
{
	var comment = await handler.Handle(command);
	return Results.Created($"/api/v1/comments/{comment.Id}", comment);
})
.WithName("CreateComment")
.WithSummary("Create a new comment")
.Produces<CommentDto>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest);

commentsApi.MapPatch("{id}", async (string id, UpdateCommentCommand command, UpdateCommentHandler handler) =>
{
	var commandWithId = command with { Id = id };
	var result = await handler.Handle(commandWithId);
	return result is not null ? Results.Ok(result) : Results.NotFound();
})
.WithName("UpdateComment")
.WithSummary("Update an existing comment")
.Produces<CommentDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status404NotFound);

commentsApi.MapDelete("{id}", async (string id, DeleteCommentHandler handler) =>
{
	var command = new DeleteCommentCommand { Id = id };
	var result = await handler.Handle(command);
	return result ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteComment")
.WithSummary("Delete (archive) a comment")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

app.Run();

