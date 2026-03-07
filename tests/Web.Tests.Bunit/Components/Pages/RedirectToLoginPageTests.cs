// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     RedirectToLoginPageTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web.Tests.Bunit
// =============================================

namespace Web.Components.Pages;

/// <summary>
/// bUnit tests for the <see cref="RedirectToLoginPage"/> Blazor page.
/// </summary>
[ExcludeFromCodeCoverage]
public class RedirectToLoginPageTests : IDisposable
{
	private readonly BunitContext _ctx;

	/// <summary>
	/// Initializes a new instance of the <see cref="RedirectToLoginPageTests"/> class.
	/// </summary>
	public RedirectToLoginPageTests()
	{
		_ctx = new BunitContext();
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		_ctx.Dispose();
		GC.SuppressFinalize(this);
	}

	[Fact]
	public void RedirectToLoginPage_NavigatesToLogin_WhenInitialized()
	{
		// Act
		_ctx.Render<RedirectToLoginPage>();

		// Assert — forceLoad navigation URI is recorded by bUnit's NavigationManager
		var nav = _ctx.Services.GetRequiredService<NavigationManager>();
		nav.Uri.Should().Contain("auth/login");
	}
}
