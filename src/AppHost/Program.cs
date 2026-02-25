using AppHost;

var builder = DistributedApplication.CreateBuilder(args);

// Configure resources
var redisCache = builder.AddRedisServices();
var mongoDb = builder.AddMongoDbServices();

var api = builder
	.AddProject<Projects.Api>("api")
	.WithReference(mongoDb).WaitFor(mongoDb);

// Web project with health check and resource dependencies
builder.AddProject<Projects.Web>("web")
		.WithExternalHttpEndpoints()
		.WithHttpHealthCheck("/health")
		.WithReference(redisCache).WaitFor(redisCache)
		.WithReference(api).WaitFor(api)
		;

builder.Build().Run();
