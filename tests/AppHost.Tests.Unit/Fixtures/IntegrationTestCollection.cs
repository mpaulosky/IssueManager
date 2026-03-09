// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     IntegrationTestCollection.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  AppHost.Tests.Unit
// =============================================

namespace AppHost.Fixtures;

/// <summary>
/// Defines the AppHostIntegration collection, backed by a shared DistributedApplicationFixture.
/// Tests in this collection share one AppHost builder instance (built once, shared across all tests).
/// </summary>
[ExcludeFromCodeCoverage]
[CollectionDefinition("AppHostIntegration")]
public sealed class AppHostIntegrationCollection : ICollectionFixture<DistributedApplicationFixture>;
