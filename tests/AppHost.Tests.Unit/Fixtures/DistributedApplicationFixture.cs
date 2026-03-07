// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DistributedApplicationFixture.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  AppHost.Tests.Unit
// =============================================

namespace AppHost.Fixtures;

/// <summary>
/// Shared fixture for AppHost integration tests that require DistributedApplicationTestingBuilder.
/// Creates the builder and app once per collection run, shared across all tests in the collection.
/// </summary>
public sealed class DistributedApplicationFixture : IAsyncLifetime
{
	private IDistributedApplicationTestingBuilder? _builder;
	private DistributedApplication? _app;

	public IDistributedApplicationTestingBuilder Builder =>
		_builder ?? throw new InvalidOperationException("Fixture not initialized. Check IsAvailable before accessing Builder.");

	public DistributedApplication App =>
		_app ?? throw new InvalidOperationException("Fixture not initialized. Check IsAvailable before accessing App.");

	/// <summary>
	/// True if the Aspire host was successfully built; false if initialization failed (e.g., Docker unavailable).
	/// </summary>
	public bool IsAvailable { get; private set; }

	/// <summary>
	/// The reason initialization was skipped or failed, if IsAvailable is false.
	/// </summary>
	public string? UnavailableReason { get; private set; }

	public async ValueTask InitializeAsync()
	{
		try
		{
			_builder = await DistributedApplicationTestingBuilder
				.CreateAsync<Projects.AppHost>(CancellationToken.None);
			_app = await _builder.BuildAsync(CancellationToken.None);
			IsAvailable = true;
		}
		catch (Exception ex)
		{
			IsAvailable = false;
			UnavailableReason = $"AppHost initialization failed: {ex.Message}";
		}
	}

	public async ValueTask DisposeAsync()
	{
		if (_app is not null)
		{
			await _app.DisposeAsync();
		}
	}
}
