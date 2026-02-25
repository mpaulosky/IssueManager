using Api.Extensions;
using Api.Handlers.Categories;
using Api.Handlers.Comments;
using Api.Handlers.Issues;
using Api.Handlers.Statuses;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.Services.AddRepositories(builder.Configuration);
builder.Services.AddValidators();
builder.Services.AddHandlers();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapOpenApi();

app.MapIssueEndpoints();
app.MapStatusEndpoints();
app.MapCategoryEndpoints();
app.MapCommentEndpoints();

app.MapDefaultEndpoints();

app.Run();

