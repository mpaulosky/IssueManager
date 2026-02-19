var builder = DistributedApplication.CreateBuilder(args);

var mongodb = builder
	.AddMongoDB("mongodb")
	.AddDatabase("issuemanager");

var api = builder
	.AddProject("api", "../Api/Api.csproj")
	.WithReference(mongodb);

builder
	.AddProject("web", "../Web/Web.csproj")
	.WithReference(api);

builder.Build().Run();
