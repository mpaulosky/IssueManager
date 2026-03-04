// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     IntegrationTestCollection.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Integration.Tests
// =======================================================
namespace Integration.Fixtures;

/// <summary>
/// Defines an xUnit collection that serializes all integration tests to prevent
/// Docker resource contention when multiple MongoDbContainers start simultaneously.
/// </summary>
[CollectionDefinition("Integration", DisableParallelization = true)]
[ExcludeFromCodeCoverage]
public class IntegrationTestCollection
{
}
