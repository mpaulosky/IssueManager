// Copyright (c) 2026. All rights reserved.

using Microsoft.AspNetCore.Components;

using Shared.DTOs;
using Shared.Validators;

using Web.Pages.Statuses;
using Web.Services;

namespace Tests.BlazorTests.Pages.Statuses;

/// <summary>
/// bUnit tests for the <see cref="CreateStatusPage"/> Blazor page.
/// </summary>
public class CreateStatusPageTests : IDisposable
{
	private readonly Bunit.TestContext _ctx;
	private readonly IStatusApiClient _mockStatusClient;

	/// <summary>
	/// Initializes a new instance of the <see cref="CreateStatusPageTests"/> class.
	/// </summary>
	public CreateStatusPageTests()
	{
		_ctx = new Bunit.TestContext();
		_mockStatusClient = Substitute.For<IStatusApiClient>();
		_mockStatusClient
			.CreateAsync(Arg.Any<CreateStatusCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<StatusDto?>(null));
		_ctx.Services.AddSingleton<IStatusApiClient>(_mockStatusClient);
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		_ctx?.Dispose();
		GC.SuppressFinalize(this);
	}

	[Fact]
	public void CreateStatusPage_RendersWithoutError_AndShowsHeading()
	{
		// Act
		var cut = _ctx.Render<CreateStatusPage>();

		// Assert
		cut.Should().NotBeNull();
		cut.Markup.Should().Contain("Create Status");
	}

	[Fact]
	public void CreateStatusPage_HasStatusNameInput()
	{
		// Act
		var cut = _ctx.Render<CreateStatusPage>();

		// Assert
		cut.Find("#status-name").Should().NotBeNull();
	}

	[Fact]
	public void CreateStatusPage_HasDescriptionInput()
	{
		// Act
		var cut = _ctx.Render<CreateStatusPage>();

		// Assert
		cut.Find("#status-description").Should().NotBeNull();
	}

	[Fact]
	public void CreateStatusPage_HasSubmitButton()
	{
		// Act
		var cut = _ctx.Render<CreateStatusPage>();

		// Assert
		var submitButton = cut.Find("button[type='submit']");
		submitButton.Should().NotBeNull();
		submitButton.TextContent.Should().Contain("Create Status");
	}

	[Fact]
	public void CreateStatusPage_HasCancelLink()
	{
		// Act
		var cut = _ctx.Render<CreateStatusPage>();

		// Assert
		var cancelLink = cut.Find("a[href='/statuses']");
		cancelLink.Should().NotBeNull();
	}

	[Fact]
	public async Task CreateStatusPage_SubmitWithValidName_CallsCreateAsync()
	{
		// Arrange
		var cut = _ctx.Render<CreateStatusPage>();
		cut.Find("#status-name").Change("New Status Name");

		// Act
		await cut.Find("form").SubmitAsync();

		// Assert
		await _mockStatusClient.Received(1)
			.CreateAsync(Arg.Any<CreateStatusCommand>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateStatusPage_AfterSuccessfulSubmit_NavigatesToStatusesList()
	{
		// Arrange
		var cut = _ctx.Render<CreateStatusPage>();
		cut.Find("#status-name").Change("Valid Status");

		// Act
		await cut.Find("form").SubmitAsync();

		// Assert
		var nav = _ctx.Services.GetRequiredService<NavigationManager>();
		nav.Uri.Should().EndWith("/statuses");
	}
}
