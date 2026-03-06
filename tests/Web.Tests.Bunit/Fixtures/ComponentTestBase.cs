// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ComponentTestBase.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web.Tests.Bunit
// =======================================================

namespace Web.Fixtures;

/// <summary>
/// Base class for Blazor component tests providing common bUnit setup and utilities.
/// </summary>
[ExcludeFromCodeCoverage]
public abstract class ComponentTestBase : IDisposable
{
	/// <summary>
	/// Gets the test context used for rendering components.
	/// </summary>
	protected BunitContext TestContext { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ComponentTestBase"/> class.
	/// </summary>
	protected ComponentTestBase()
	{
		TestContext = new BunitContext();
		TestContext.AddAuthorization();

		var categoryClient = Substitute.For<ICategoryApiClient>();
		categoryClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CategoryDto>>([]));
		TestContext.Services.AddSingleton(categoryClient);

		var statusClient = Substitute.For<IStatusApiClient>();
		statusClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<StatusDto>>([]));
		TestContext.Services.AddSingleton(statusClient);
	}

	/// <summary>
	/// Disposes the test context after each test.
	/// </summary>
	public void Dispose()
	{
		TestContext.Dispose();
		GC.SuppressFinalize(this);
	}
}
