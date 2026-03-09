// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     AppHostTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  AppHost.Tests.Unit
// =============================================

namespace AppHost;

/// <summary>
/// Integration tests for the AppHost orchestration and resource configuration.
/// Uses a shared DistributedApplicationFixture so the Aspire host is built once per run.
/// Tests skip gracefully when Aspire/Docker initialization fails.
/// </summary>
/// <remarks>
/// Private methods (OnRunClearCacheCommandAsync, OnUpdateResourceState, WithClearCommand) cannot
/// be tested directly as they are invoked internally by Aspire's command infrastructure.
/// </remarks>
[ExcludeFromCodeCoverage]
[Collection("AppHostIntegration")]
public class AppHostTests(DistributedApplicationFixture fixture)
{
	/// <summary>
	/// Verifies that the AppHost can be created and built successfully.
	/// </summary>
	[Fact]
	public void AppHost_CanBeCreatedAndBuilt()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Aspire host unavailable");

		// Act & Assert
		fixture.App.Should().NotBeNull();
	}

	/// <summary>
	/// Verifies that MongoDB resources are correctly registered in the AppHost.
	/// </summary>
	[Fact]
	public void AppHost_RegistersMongoDbResource()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Aspire host unavailable");

		// Act
		var resources = fixture.Builder.Resources;

		// Assert
		resources.Should().Contain(r => r.Name == DatabaseName, "MongoDB Atlas connection string resource should be registered");
	}

	/// <summary>
	/// Verifies that Redis resources are correctly registered in the AppHost.
	/// </summary>
	[Fact]
	public void AppHost_RegistersRedisResource()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Aspire host unavailable");

		// Act
		var resources = fixture.Builder.Resources;

		// Assert
		resources.Should().Contain(r => r.Name == "RedisCache", "Redis cache resource should be registered");
	}

	/// <summary>
	/// Verifies that the AppHost registers the expected API service.
	/// </summary>
	[Fact]
	public void AppHost_RegistersApiService()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Aspire host unavailable");

		// Act
		var resources = fixture.Builder.Resources;

		// Assert
		resources.Should().Contain(r => r.Name == ApiService, "API service should be registered");
	}

	/// <summary>
	/// Verifies that the AppHost registers the expected web application service.
	/// </summary>
	[Fact]
	public void AppHost_RegistersWebService()
	{
		// Arrange
		if (!fixture.IsAvailable)
			throw SkipException.ForSkip(fixture.UnavailableReason ?? "Aspire host unavailable");

		// Act
		var resources = fixture.Builder.Resources;

		// Assert
		resources.Should().Contain(r => r.Name == Website, "Web application should be registered");
	}
}

