// Copyright (c) 2026. All rights reserved.

namespace Tests.BlazorTests.Pages.Issues;

/// <summary>
/// bUnit tests for the <see cref="CreateIssuePage"/> Blazor page.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreateIssuePageTests : ComponentTestBase
{
	private readonly IIssueApiClient _mockIssueClient;

	/// <summary>
	/// Initializes a new instance of the <see cref="CreateIssuePageTests"/> class.
	/// </summary>
	public CreateIssuePageTests()
	{
		_mockIssueClient = Substitute.For<IIssueApiClient>();
		_mockIssueClient
			.CreateAsync(Arg.Any<CreateIssueCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(null));
		TestContext.Services.AddSingleton<IIssueApiClient>(_mockIssueClient);
	}

	[Fact]
	public void CreateIssuePage_RendersWithoutError_AndShowsHeading()
	{
		// Act
		var cut = TestContext.Render<CreateIssuePage>();

		// Assert
		cut.Should().NotBeNull();
		cut.Markup.Should().Contain("Create Issue");
	}

	[Fact]
	public void CreateIssuePage_RendersIssueForm()
	{
		// Act
		var cut = TestContext.Render<CreateIssuePage>();

		// Assert
		cut.FindComponent<IssueForm>().Should().NotBeNull();
	}

	[Fact]
	public void CreateIssuePage_IssueFormHasSubmitButton()
	{
		// Act
		var cut = TestContext.Render<CreateIssuePage>();

		// Assert
		var submitButton = cut.Find("button[type='submit']");
		submitButton.Should().NotBeNull();
	}

	[Fact]
	public async Task CreateIssuePage_SubmitWithValidData_CallsCreateAsync()
	{
		// Arrange
		var cut = TestContext.Render<CreateIssuePage>();
		cut.Find("#title").Change("A Valid Issue Title");
		cut.Find("#description").Change("Some description text");

		// Act
		await cut.Find("form").SubmitAsync();

		// Assert
		await _mockIssueClient.Received(1)
			.CreateAsync(Arg.Any<CreateIssueCommand>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task CreateIssuePage_AfterSuccessfulSubmit_NavigatesToIssuesList()
	{
		// Arrange
		var cut = TestContext.Render<CreateIssuePage>();
		cut.Find("#title").Change("A Valid Issue Title");
		cut.Find("#description").Change("Some description");

		// Act
		await cut.Find("form").SubmitAsync();

		// Assert
		var nav = TestContext.Services.GetRequiredService<NavigationManager>();
		nav.Uri.Should().EndWith("/issues");
	}
}
