using Bunit;

namespace IssueManager.Tests.BlazorTests.Fixtures;

/// <summary>
/// Base class for Blazor component tests providing common bUnit setup and utilities.
/// </summary>
public abstract class ComponentTestBase : IDisposable
{
	/// <summary>
	/// Gets the test context used for rendering components.
	/// </summary>
	protected TestContext TestContext { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ComponentTestBase"/> class.
	/// </summary>
	protected ComponentTestBase()
	{
		TestContext = new TestContext();
		
		// Add common test services here
		// Example: TestContext.Services.AddSingleton<IMyService>(NSubstitute.Substitute.For<IMyService>());
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
