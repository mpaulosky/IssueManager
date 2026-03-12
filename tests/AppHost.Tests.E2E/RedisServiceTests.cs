// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     RedisServiceTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  AppHost.Tests.E2E
// =======================================================

namespace AppHost.Tests.E2E;

/// <summary>
/// Unit tests for RedisServices extension methods.
/// </summary>
/// <remarks>
/// Tests focus on verifying the public method can be called and returns expected types.
/// Private methods (OnRunClearCacheCommandAsync, OnUpdateResourceState, WithClearCommand)
/// cannot be tested directly as they are invoked internally by Aspire's command infrastructure.
/// Full integration testing is done in AppHostTests.cs.
/// </remarks>
[ExcludeFromCodeCoverage]
public class RedisServiceTests
{
	/// <summary>
	/// Verifies that AddRedisServices can be invoked and returns a resource builder.
	/// </summary>
	[Fact]
	public void AddRedisServices_CanBeInvoked()
	{
		// Arrange
		var builder = DistributedApplication.CreateBuilder();

		// Act
		var result = AppHost.RedisServices.AddRedisServices(builder);

		// Assert
		result.Should().NotBeNull("method should return a resource builder");
		result.Should().BeAssignableTo<IResourceBuilder<RedisResource>>(
		"should return Redis resource builder");
	}

	/// <summary>
	/// Verifies that AddRedisServices registers a Redis resource with the expected name.
	/// </summary>
	[Fact]
	public void AddRedisServices_RegistersResourceWithCorrectName()
	{
		// Arrange
		var builder = DistributedApplication.CreateBuilder();

		// Act
		var result = AppHost.RedisServices.AddRedisServices(builder);

		// Assert
		result.Resource.Name.Should().Be("RedisCache", "should register Redis with the correct name from constants");
	}
}
