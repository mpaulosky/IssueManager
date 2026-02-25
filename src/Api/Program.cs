using ServiceDefaults;
using Api.Data;
using Api.Handlers;
using Api.Handlers.Issues;
using Api.Handlers.Statuses;
using Api.Handlers.Categories;
using Api.Handlers.Comments;
using Shared.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

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

app.MapIssueEndpoints();
app.MapStatusEndpoints();
app.MapCategoryEndpoints();
app.MapCommentEndpoints();

app.MapDefaultEndpoints();

app.Run();

