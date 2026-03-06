// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     AppHostTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Aspire.Tests
// =======================================================

namespace AppHost;

/// <summary>
/// Integration tests for the AppHost orchestration and resource configuration.
/// </summary>
/// <remarks>
/// These tests verify that the AppHost can build and start with correct resource registrations.
/// Private methods (OnRunClearCacheCommandAsync, OnUpdateResourceState, WithClearCommand) cannot
/// be tested directly as they are invoked internally by Aspire's command infrastructure.
/// </remarks>
public class AppHostTests
{
	/// <summary>
	/// Verifies that the AppHost can be created and built successfully.
	/// </summary>
	[Fact]
	public async Task AppHost_CanBeCreatedAndBuilt()
	{
		// Arrange & Act
		var builder = await DistributedApplicationTestingBuilder
		.CreateAsync<Projects.AppHost>(TestContext.Current.CancellationToken);

		await using var app = await builder.BuildAsync(TestContext.Current.CancellationToken);

		// Assert
		app.Should().NotBeNull();
	}

	/// <summary>
	/// Verifies that MongoDB resources are correctly registered in the AppHost.
	/// </summary>
	[Fact]
	public async Task AppHost_RegistersMongoDbResource()
	{
		// Arrange
		var builder = await DistributedApplicationTestingBuilder
		.CreateAsync<Projects.AppHost>(TestContext.Current.CancellationToken);

		// Act
		var resources = builder.Resources;

		// Assert
		resources.Should().Contain(r => r.Name == DatabaseName, "MongoDB Atlas connection string resource should be registered");
	}

	/// <summary>
	/// Verifies that Redis resources are correctly registered in the AppHost.
	/// </summary>
	[Fact]
	public async Task AppHost_RegistersRedisResource()
	{
		// Arrange
		var builder = await DistributedApplicationTestingBuilder
		.CreateAsync<Projects.AppHost>(TestContext.Current.CancellationToken);

		// Act
		var resources = builder.Resources;

		// Assert
		resources.Should().Contain(r => r.Name == "RedisCache", "Redis cache resource should be registered");
	}

	/// <summary>
	/// Verifies that the AppHost registers the expected API service.
	/// </summary>
	[Fact]
	public async Task AppHost_RegistersApiService()
	{
		// Arrange
		var builder = await DistributedApplicationTestingBuilder
		.CreateAsync<Projects.AppHost>(TestContext.Current.CancellationToken);

		// Act
		var resources = builder.Resources;

		// Assert
		resources.Should().Contain(r => r.Name == ApiService, "API service should be registered");
	}

	/// <summary>
	/// Verifies that the AppHost registers the expected web application service.
	/// </summary>
	[Fact]
	public async Task AppHost_RegistersWebService()
	{
		// Arrange
		var builder = await DistributedApplicationTestingBuilder
		.CreateAsync<Projects.AppHost>(TestContext.Current.CancellationToken);

		// Act
		var resources = builder.Resources;

		// Assert
		resources.Should().Contain(r => r.Name == Website, "Web application should be registered");
	}
}
