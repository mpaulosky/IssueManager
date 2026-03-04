// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     MongoDbFixture.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Integration.Tests
// =======================================================
namespace Integration.Fixtures;

/// <summary>
/// Shared MongoDB TestContainer fixture for integration tests.
/// </summary>
[ExcludeFromCodeCoverage]
public class MongoDbFixture : IAsyncLifetime
{
	private const string MongodbImage = "mongo:latest";
	private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder(MongodbImage)
			.Build();

	/// <summary>
	/// Gets the MongoDB connection string.
	/// </summary>
	public string ConnectionString => _mongoContainer.GetConnectionString();

	/// <summary>
	/// Initializes the MongoDB container.
	/// </summary>
	public async ValueTask InitializeAsync()
	{
		await _mongoContainer.StartAsync();
	}

	/// <summary>
	/// Disposes the MongoDB container.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		await _mongoContainer.StopAsync();
		await _mongoContainer.DisposeAsync();
	}
}
