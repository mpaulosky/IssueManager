// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     DatabaseService.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  AppHost
// =======================================================

namespace AppHost;

/// <summary>
///   Extension methods for adding and configuring MongoDB resources with Aspire latest features.
/// </summary>
public static class DatabaseService
{

	/// <summary>
	///   Adds MongoDB services to the distributed application builder, including resource tagging, grouping, and improved
	///   seeding logic.
	/// </summary>
	/// <param name="builder">The distributed application builder.</param>
	/// <returns>The MongoDB database resource builder.</returns>
	public static IResourceBuilder<MongoDBDatabaseResource> AddMongoDbServices(
		this IDistributedApplicationBuilder builder)
	{

		using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
		var logger = loggerFactory.CreateLogger(nameof(DatabaseService));

		var mongoDbConnection = builder.AddParameter("mongoDb-connection", secret: true);

		// Determine the database name based on the environment
		var isDevelopment = builder.Environment.IsDevelopment();
		var databaseName = isDevelopment ? DevDatabaseName : DatabaseName;

		var environmentName = isDevelopment ? "Development" : "Production";

		logger.LogInformation("MongoDB configured for {Environment} environment with database: {DatabaseName}",
			environmentName, databaseName);

		// Use a valid resource name, not the connection string
		logger.LogDebug("Configuring MongoDB server resource: {ServerName}", Server);
		var server = builder.AddMongoDB(Server)
				.WithLifetime(ContainerLifetime.Persistent)
				.WithDataVolume("mongodb-data", isReadOnly: false)
				.WithEnvironment("MONGODB-CONNECTION-STRING", mongoDbConnection)
				.WithEnvironment("MONGODB-DATABASE-NAME", databaseName)
				.WithMongoExpress();

		logger.LogDebug("Creating MongoDB database resource: {DatabaseName}", databaseName);
		var database = server.AddDatabase(databaseName);

		logger.LogInformation("MongoDB resources configured successfully: server={ServerName}, database={DatabaseName}",
			Server, databaseName);

		return database;

	}

}
