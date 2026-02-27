// Copyright (c) 2026. All rights reserved.

using Microsoft.AspNetCore.Components;

using MongoDB.Bson;

using Shared.DTOs;
using Shared.Validators;

using Tests.BlazorTests.Fixtures;

using Web.Pages.Issues;
using Web.Services;

namespace Tests.BlazorTests.Pages.Issues;

/// <summary>
/// bUnit tests for the <see cref="EditIssuePage"/> Blazor page.
/// </summary>
public class EditIssuePageTests : ComponentTestBase
{
	private readonly IIssueApiClient _mockIssueClient;

	/// <summary>
	/// Initializes a new instance of the <see cref="EditIssuePageTests"/> class.
	/// </summary>
	public EditIssuePageTests()
	{
		_mockIssueClient = Substitute.For<IIssueApiClient>();
		_mockIssueClient
			.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(null));
		_mockIssueClient
			.UpdateAsync(Arg.Any<string>(), Arg.Any<UpdateIssueCommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(null));
		TestContext.Services.AddSingleton<IIssueApiClient>(_mockIssueClient);
	}

	private static IssueDto MakeIssue(string title = "Existing Issue") => new(
		ObjectId.GenerateNewId(),
		title,
		"Existing description",
		DateTime.UtcNow,
		null,
		new UserDto("user1", "Author Name", "author@test.com"),
		new CategoryDto(ObjectId.GenerateNewId(), "Feature", "Feature category", DateTime.UtcNow, null, false, UserDto.Empty),
		new StatusDto(ObjectId.GenerateNewId(), "Open", "Open status", DateTime.UtcNow, null, false, UserDto.Empty),
		false,
		UserDto.Empty,
		false,
		false);

	[Fact]
	public void EditIssuePage_ShowsNotFoundMessage_WhenIssueIsNull()
	{
		// Arrange — mock returns null by default

		// Act
		var cut = TestContext.RenderComponent<EditIssuePage>(p =>
			p.Add(c => c.Id, "nonexistent-id"));

		// Assert
		cut.Markup.Should().Contain("Issue not found.");
	}

	[Fact]
	public void EditIssuePage_ShowsEditHeading_WhenIssueLoads()
	{
		// Arrange
		var issue = MakeIssue();
		_mockIssueClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(issue));

		// Act
		var cut = TestContext.RenderComponent<EditIssuePage>(p =>
			p.Add(c => c.Id, issue.Id.ToString()));

		// Assert
		cut.Markup.Should().Contain("Edit Issue");
	}

	[Fact]
	public void EditIssuePage_RendersIssueForm_WhenIssueLoads()
	{
		// Arrange
		var issue = MakeIssue();
		_mockIssueClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(issue));

		// Act
		var cut = TestContext.RenderComponent<EditIssuePage>(p =>
			p.Add(c => c.Id, issue.Id.ToString()));

		// Assert
		cut.FindComponent<IssueForm>().Should().NotBeNull();
	}

	[Fact]
	public void EditIssuePage_PrePopulatesFormWithIssueTitleAsync()
	{
		// Arrange
		var issue = MakeIssue("Pre-populated Title");
		_mockIssueClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(issue));

		// Act
		var cut = TestContext.RenderComponent<EditIssuePage>(p =>
			p.Add(c => c.Id, issue.Id.ToString()));

		// Assert
		var titleInput = cut.Find("#title");
		titleInput.GetAttribute("value").Should().Be("Pre-populated Title");
	}

	[Fact]
	public void EditIssuePage_FormIsInEditMode_WhenIssueLoads()
	{
		// Arrange
		var issue = MakeIssue();
		_mockIssueClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(issue));

		// Act
		var cut = TestContext.RenderComponent<EditIssuePage>(p =>
			p.Add(c => c.Id, issue.Id.ToString()));

		// Assert — edit mode shows "Update Issue" on the submit button
		cut.Find("button[type='submit']").TextContent.Should().Contain("Update Issue");
	}

	[Fact]
	public async Task EditIssuePage_SubmitForm_CallsUpdateAsync()
	{
		// Arrange
		var issue = MakeIssue("Editable Issue");
		_mockIssueClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(issue));

		var cut = TestContext.RenderComponent<EditIssuePage>(p =>
			p.Add(c => c.Id, issue.Id.ToString()));

		// Act — update the title and submit
		cut.Find("#title").Change("Updated Issue Title");
		await cut.Find("form").SubmitAsync();

		// Assert
		await _mockIssueClient.Received(1)
			.UpdateAsync(Arg.Any<string>(), Arg.Any<UpdateIssueCommand>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task EditIssuePage_AfterSuccessfulUpdate_NavigatesToIssueDetail()
	{
		// Arrange
		var issue = MakeIssue("Issue To Update");
		_mockIssueClient.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult<IssueDto?>(issue));

		var cut = TestContext.RenderComponent<EditIssuePage>(p =>
			p.Add(c => c.Id, issue.Id.ToString()));

		cut.Find("#title").Change("Updated Title");

		// Act
		await cut.Find("form").SubmitAsync();

		// Assert
		var nav = TestContext.Services.GetRequiredService<NavigationManager>();
		nav.Uri.Should().Contain($"/issues/{issue.Id}");
	}
}
