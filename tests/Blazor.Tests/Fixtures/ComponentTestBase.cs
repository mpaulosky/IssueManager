using Bunit;
using NSubstitute;
using Shared.DTOs;
using Web.Services;

namespace Tests.BlazorTests.Fixtures;

/// <summary>
/// Base class for Blazor component tests providing common bUnit setup and utilities.
/// </summary>
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

		var categoryClient = Substitute.For<ICategoryApiClient>();
		categoryClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CategoryDto>>([]));
		TestContext.Services.AddSingleton<ICategoryApiClient>(categoryClient);

		var statusClient = Substitute.For<IStatusApiClient>();
		statusClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<StatusDto>>([]));
		TestContext.Services.AddSingleton<IStatusApiClient>(statusClient);
	}

	/// <summary>
	/// Disposes the test context after each test.
	/// </summary>
	public void Dispose()
	{
		TestContext?.Dispose();
		GC.SuppressFinalize(this);
	}
}
