// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DatabaseServiceTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Aspire.Tests
// =======================================================

using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using FluentAssertions;
using static Shared.Constants.Constants;

namespace Aspire.Tests;

/// <summary>
/// Unit tests for DatabaseService extension methods.
/// </summary>
/// <remarks>
/// Tests focus on verifying the method can be called and returns expected types.
/// Full integration testing is done in AppHostTests.cs.
/// </remarks>
public class DatabaseServiceTests
{
/// <summary>
/// Verifies that AddMongoDbServices can be invoked and returns a resource builder.
/// </summary>
[Fact]
public void AddMongoDbServices_CanBeInvoked()
{
// Arrange
var builder = DistributedApplication.CreateBuilder();

// Act
var result = AppHost.DatabaseService.AddMongoDbServices(builder);

// Assert
result.Should().NotBeNull("method should return a resource builder");
result.Should().BeAssignableTo<IResourceBuilder<MongoDBDatabaseResource>>(
"should return MongoDB database resource builder");
}

/// <summary>
/// Verifies that AddMongoDbServices uses the correct database name in development environment.
/// </summary>
[Fact]
public void AddMongoDbServices_UsesDevelopmentDatabaseName_InDevelopmentEnvironment()
{
// Arrange
var args = new string[] { "--environment", "Development" };
var builder = DistributedApplication.CreateBuilder(args);

// Act
var result = AppHost.DatabaseService.AddMongoDbServices(builder);

// Assert
result.Should().NotBeNull();
result.Resource.Name.Should().Be(DevDatabaseName, "should use dev database name in development");
}

/// <summary>
/// Verifies that AddMongoDbServices uses the correct database name in production environment.
/// </summary>
[Fact]
public void AddMongoDbServices_UsesProductionDatabaseName_InProductionEnvironment()
{
// Arrange
var args = new string[] { "--environment", "Production" };
var builder = DistributedApplication.CreateBuilder(args);

// Act
var result = AppHost.DatabaseService.AddMongoDbServices(builder);

// Assert
result.Should().NotBeNull();
result.Resource.Name.Should().Be(DatabaseName, "should use production database name in production");
}
}