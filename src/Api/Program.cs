using IssueManager.ServiceDefaults;
using IssueManager.Api.Data;
using IssueManager.Api.Handlers;

using Shared.Validators;

using static IssueManager.Api.Handlers.GetIssueHandler;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceDefaults();

builder.Services.AddOpenApi();

// Register HttpContextAccessor for accessing current user context
builder.Services.AddHttpContextAccessor();

// Register repository
var connectionString = builder.Configuration.GetConnectionString("IssueManagerDb") 
	?? "mongodb://localhost:27017";
builder.Services.AddSingleton<IIssueRepository>(sp => 
	new IssueRepository(connectionString, "IssueManagerDb"));

// Register validators
builder.Services.AddSingleton<CreateIssueValidator>();
builder.Services.AddSingleton<UpdateIssueValidator>();
builder.Services.AddSingleton<DeleteIssueValidator>();
builder.Services.AddSingleton<ListIssuesQueryValidator>();
builder.Services.AddSingleton<UpdateIssueStatusValidator>();

// Register handlers
builder.Services.AddSingleton<CreateIssueHandler>();
builder.Services.AddSingleton<UpdateIssueHandler>();
builder.Services.AddSingleton<DeleteIssueHandler>();
builder.Services.AddSingleton<ListIssuesHandler>();
builder.Services.AddSingleton<GetIssueHandler>();
builder.Services.AddSingleton<UpdateIssueStatusHandler>();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapOpenApi();

// Issue API Endpoints
var issuesApi = app.MapGroup("/api/v1/issues")
	.WithTags("Issues")
	.WithOpenApi();

// List Issues (paginated)
issuesApi.MapGet("", async (int? page, int? pageSize, ListIssuesHandler handler) =>
{
	var query = new ListIssuesQuery { Page = page ?? 1, PageSize = pageSize ?? 20 };
	var result = await handler.Handle(query);
	return Results.Ok(result);
})
.WithName("ListIssues")
.WithSummary("Get a paginated list of issues")
.Produces<PaginatedResponse<IssueResponseDto>>(StatusCodes.Status200OK)
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

app.Run();

