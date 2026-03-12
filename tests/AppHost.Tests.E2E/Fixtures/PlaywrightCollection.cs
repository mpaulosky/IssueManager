// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     PlaywrightCollection.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  AppHost.Tests.E2E
// =============================================

namespace AppHost.Tests.E2E.Fixtures;

/// <summary>
/// Defines the PlaywrightE2E collection, backed by a shared PlaywrightFixture.
/// Tests in this collection share one Aspire host + Playwright browser instance.
/// </summary>
[ExcludeFromCodeCoverage]
[CollectionDefinition("PlaywrightE2E")]
public sealed class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>;
