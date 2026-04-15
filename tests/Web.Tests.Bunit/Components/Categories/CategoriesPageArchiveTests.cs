// =============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CategoriesPageArchiveTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web.Tests.Bunit
// =============================================

namespace Web.Components.Features.Categories;

/// <summary>
/// bUnit tests verifying archive action behavior in <see cref="CategoriesPage"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class CategoriesPageArchiveTests : IDisposable
{
	private readonly BunitContext _ctx;
	private readonly ICategoryApiClient _mockCategoryClient;

	/// <summary>
	/// Initializes a new instance of the <see cref="CategoriesPageArchiveTests"/> class.
	/// </summary>
	public CategoriesPageArchiveTests()
	{
		_ctx = new BunitContext();
		_ctx.JSInterop.Mode = JSRuntimeMode.Loose;
		_mockCategoryClient = Substitute.For<ICategoryApiClient>();
		_mockCategoryClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CategoryDto>>([]));
		_ctx.Services.AddSingleton(_mockCategoryClient);
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		_ctx.Dispose();
		GC.SuppressFinalize(this);
	}

	private static CategoryDto MakeCategory(string name = "Bug", string description = "Bug fixes") => new(
		ObjectId.GenerateNewId(),
		name,
		description,
		DateTime.UtcNow,
		null,
		false,
		UserDto.Empty);

	// ─── Admin Guard ─────────────────────────────────────────────────────────────

	[Fact]
	public void ArchiveButton_AdminUser_IsVisible()
	{
		// Arrange
		var category = MakeCategory("Performance");
		_mockCategoryClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CategoryDto>>([category]));
		_ctx.AddAuthorization().SetAuthorized("Admin User").SetRoles("Admin");

		// Act
		var cut = _ctx.Render<CategoriesPage>();

		// Assert
		cut.Find($"#archive-{category.Id}").Should().NotBeNull();
	}

	[Fact]
	public void ArchiveButton_NonAdminUser_IsNotVisible()
	{
		// Arrange
		var category = MakeCategory("Performance");
		_mockCategoryClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CategoryDto>>([category]));
		_ctx.AddAuthorization().SetAuthorized("Regular User");

		// Act
		var cut = _ctx.Render<CategoriesPage>();

		// Assert — archive button must not be present for non-admin users
		cut.FindAll($"#archive-{category.Id}").Should().BeEmpty();
	}

	// ─── Confirmation Dialog ──────────────────────────────────────────────────────

	[Fact]
	public async Task ArchiveButton_Clicked_ShowsConfirmDialog()
	{
		// Arrange
		var category = MakeCategory("Security");
		_mockCategoryClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CategoryDto>>([category]));
		_ctx.AddAuthorization().SetAuthorized("Admin User").SetRoles("Admin");
		var cut = _ctx.Render<CategoriesPage>();

		// Act
		await cut.Find($"#archive-{category.Id}").ClickAsync(new MouseEventArgs());

		// Assert
		cut.Find("[role='dialog']").Should().NotBeNull();
	}

	[Fact]
	public async Task ConfirmDialog_Confirmed_CallsArchiveApiAndRemovesRow()
	{
		// Arrange
		var category = MakeCategory("Obsolete");
		_mockCategoryClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CategoryDto>>([category]));
		_mockCategoryClient.ArchiveAsync(category.Id.ToString(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(true));
		_ctx.AddAuthorization().SetAuthorized("Admin User").SetRoles("Admin");
		var cut = _ctx.Render<CategoriesPage>();

		// Open dialog
		await cut.Find($"#archive-{category.Id}").ClickAsync(new MouseEventArgs());

		// Act — click the confirm button
		var confirmButton = cut.FindAll("button").First(b => b.TextContent.Contains("Yes, Archive"));
		await confirmButton.ClickAsync(new MouseEventArgs());

		// Assert
		await _mockCategoryClient.Received(1).ArchiveAsync(category.Id.ToString(), Arg.Any<CancellationToken>());
		cut.Markup.Should().NotContain("Obsolete");
	}

	[Fact]
	public async Task ConfirmDialog_Cancelled_DoesNotCallApi()
	{
		// Arrange
		var category = MakeCategory("Keep Me");
		_mockCategoryClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CategoryDto>>([category]));
		_ctx.AddAuthorization().SetAuthorized("Admin User").SetRoles("Admin");
		var cut = _ctx.Render<CategoriesPage>();

		// Open dialog
		await cut.Find($"#archive-{category.Id}").ClickAsync(new MouseEventArgs());

		// Act — click the cancel button
		var cancelButton = cut.FindAll("button").First(b => b.TextContent.Contains("Cancel"));
		await cancelButton.ClickAsync(new MouseEventArgs());

		// Assert — API must not have been called and dialog must be hidden
		await _mockCategoryClient.DidNotReceive().ArchiveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
		cut.FindAll("[role='dialog']").Should().BeEmpty();
	}

	// ─── Error Handling ───────────────────────────────────────────────────────────

	[Fact]
	public async Task ArchiveApi_ReturnsError_ShowsErrorMessage()
	{
		// Arrange
		var category = MakeCategory("Failing");
		_mockCategoryClient.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IEnumerable<CategoryDto>>([category]));
		_mockCategoryClient.ArchiveAsync(category.Id.ToString(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(false));
		_ctx.AddAuthorization().SetAuthorized("Admin User").SetRoles("Admin");
		var cut = _ctx.Render<CategoriesPage>();

		// Open dialog and confirm
		await cut.Find($"#archive-{category.Id}").ClickAsync(new MouseEventArgs());
		var confirmButton = cut.FindAll("button").First(b => b.TextContent.Contains("Yes, Archive"));
		await confirmButton.ClickAsync(new MouseEventArgs());

		// Assert — error element is shown
		cut.Find("#archive-error").Should().NotBeNull();
		cut.Find("#archive-error").TextContent.Should().NotBeNullOrWhiteSpace();
	}
}
