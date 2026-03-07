// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     IntegrationTestCollection.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Integration
// =======================================================
namespace Integration.Fixtures;

[CollectionDefinition("CategoryIntegration")]
[ExcludeFromCodeCoverage]
public class CategoryIntegrationCollection : ICollectionFixture<MongoDbFixture> { }

[CollectionDefinition("IssueIntegration")]
[ExcludeFromCodeCoverage]
public class IssueIntegrationCollection : ICollectionFixture<MongoDbFixture> { }

[CollectionDefinition("CommentIntegration")]
[ExcludeFromCodeCoverage]
public class CommentIntegrationCollection : ICollectionFixture<MongoDbFixture> { }

[CollectionDefinition("StatusIntegration")]
[ExcludeFromCodeCoverage]
public class StatusIntegrationCollection : ICollectionFixture<MongoDbFixture> { }
