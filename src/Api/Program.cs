// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     Program.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddAuth0();
builder.AddApiVersioning();

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
	{
		var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
			?? ["https://localhost:7480", "http://localhost:5542"];

		policy.WithOrigins(allowedOrigins)
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});

builder.Services.AddRepositories(builder.Configuration);
builder.Services.AddValidators();
builder.Services.AddHandlers();
builder.Services.AddCurrentUser();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapOpenApi();
app.MapScalarApiReference();

app.MapIssueEndpoints();
app.MapStatusEndpoints();
app.MapCategoryEndpoints();
app.MapCommentEndpoints();

app.MapDefaultEndpoints();

app.Run();
