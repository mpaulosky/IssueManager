// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DatabaseServiceTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  AppHost.Tests.Unit
// =======================================================

// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DatabaseServiceTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  AppHost.Tests.Unit
// =======================================================

namespace AppHost;

/// <summary>
/// Unit tests for database connection string registration in the AppHost.
/// </summary>
/// <remarks>
/// MongoDB is registered as an external Atlas URI connection string via
/// <c>builder.AddConnectionString(DatabaseName)</c> rather than as a managed container.
/// Full integration testing is done in AppHostTests.cs.
/// </remarks>
public class DatabaseServiceTests
{
	/// <summary>
	/// Verifies that AddConnectionString can be invoked and returns a resource builder.
	/// </summary>
	[Fact]
	public void AddConnectionString_CanBeInvoked()
	{
		// Arrange
		var builder = DistributedApplication.CreateBuilder();

		// Act
		var result = builder.AddConnectionString(DatabaseName);

		// Assert
		result.Should().NotBeNull("AddConnectionString should return a resource builder");
		result.Should().BeAssignableTo<IResourceBuilder<IResourceWithConnectionString>>(
			"should return a connection string resource builder for the Atlas URI");
	}

	/// <summary>
	/// Verifies that the connection string resource uses the correct database name.
	/// </summary>
	[Fact]
	public void AddConnectionString_UsesConfiguredDatabaseName()
	{
		// Arrange
		var builder = DistributedApplication.CreateBuilder();

		// Act
		var result = builder.AddConnectionString(DatabaseName);

		// Assert
		result.Should().NotBeNull();
		result.Resource.Name.Should().Be(DatabaseName, "resource name must match the database constant");
	}

	/// <summary>
	/// Verifies that the Atlas URI connection string resource is registered in the builder.
	/// </summary>
	[Fact]
	public void AddConnectionString_ResourceIsRegistered()
	{
		// Arrange
		var builder = DistributedApplication.CreateBuilder();

		// Act
		builder.AddConnectionString(DatabaseName);

		// Assert
		builder.Resources.Should().Contain(r => r.Name == DatabaseName,
			"the database connection string resource should be registered with the builder");
	}
}
