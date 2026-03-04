// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     Program.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  AppHost
// =======================================================
using AppHost;

var builder = DistributedApplication.CreateBuilder(args);

// Register the Atlas URI as an external connection string — no container, no health check churn.
// Set the Atlas URI in user secrets: ConnectionStrings:{databaseName}
// The resource name matches databaseName so referencing projects receive
// ConnectionStrings:{databaseName} = <Atlas URI>, which aligns with what the API reads.
var database = builder.AddConnectionString(DatabaseName);

// Configure resources
var redisCache = builder.AddRedisServices();
//var mongoDb = builder.AddMongoDbServices();

var api = builder
		.AddProject<Projects.Api>(ApiService)
		.WithExternalHttpEndpoints()
		.WithReference(database); // Atlas URI connection string;

// Web project with health check and resource dependencies
builder.AddProject<Projects.Web>(Website)
		.WithExternalHttpEndpoints()
		.WithHttpHealthCheck("/health")
		.WithReference(redisCache).WaitFor(redisCache)
		.WithReference(api).WaitFor(api);

builder.Build().Run();
