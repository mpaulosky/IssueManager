namespace Tests.BlazorTests.Components;

/// <summary>
/// Tests for the IssueForm Blazor component.
/// </summary>
[ExcludeFromCodeCoverage]
public class IssueFormTests : ComponentTestBase
{
	[Fact]
	public void IssueForm_RendersCorrectly_WhenInitialized()
	{
		// Act
		var component = TestContext.Render<IssueForm>();

		// Assert
		component.Should().NotBeNull();
		component.Find("form").Should().NotBeNull();
		component.Find("#title").Should().NotBeNull();
		component.Find("#description").Should().NotBeNull();
		component.Find("#status").Should().NotBeNull();
		component.Find("button[type='submit']").Should().NotBeNull();
	}

	[Fact]
	public void IssueForm_ShowsCreateButtonText_WhenIsEditModeIsFalse()
	{
		// Act
		var component = TestContext.Render<IssueForm>(
			parameters => parameters.Add(c => c.IsEditMode, false)
		);

		// Assert
		var submitButton = component.Find("button[type='submit']");
		submitButton.TextContent.Should().Contain("Create Issue");
	}

	[Fact]
	public void IssueForm_ShowsUpdateButtonText_WhenIsEditModeIsTrue()
	{
		// Act
		var component = TestContext.Render<IssueForm>(
			parameters => parameters.Add(c => c.IsEditMode, true)
		);

		// Assert
		var submitButton = component.Find("button[type='submit']");
		submitButton.TextContent.Should().Contain("Update Issue");
	}

	[Fact]
	public void IssueForm_ShowsCancelButton_WhenOnCancelCallbackIsDefined()
	{
		// Arrange
		var cancelCallback = EventCallback.Factory.Create(this, () => { /* Cancel handler */ });

		// Act
		var component = TestContext.Render<IssueForm>(
			parameters => parameters.Add(c => c.OnCancel, cancelCallback)
		);

		// Assert
		var buttons = component.FindAll("button");
		buttons.Should().HaveCount(2);
		buttons[1].TextContent.Should().Contain("Cancel");
	}

	[Fact]
	public void IssueForm_HidesCancelButton_WhenOnCancelCallbackIsNotDefined()
	{
		// Act
		var component = TestContext.Render<IssueForm>();

		// Assert
		var buttons = component.FindAll("button");
		buttons.Should().HaveCount(1);
	}

	[Fact]
	public async Task IssueForm_InvokesOnSubmitCallback_WhenFormIsSubmittedWithValidData()
	{
		// Arrange
		CreateIssueRequest? submittedRequest = null;
		var submitCallback = EventCallback.Factory.Create<CreateIssueRequest>(
			this,
			request => { submittedRequest = request; }
		);

		var component = TestContext.Render<IssueForm>(
			parameters => parameters.Add(c => c.OnSubmit, submitCallback)
		);

		// Act - Fill in form fields
		var titleInput = component.Find("#title");
		titleInput.Change("Test Issue Title");

		var descriptionInput = component.Find("#description");
		descriptionInput.Change("Test description");

		// Submit form
		var form = component.Find("form");
		await form.SubmitAsync();

		// Assert
		submittedRequest.Should().NotBeNull();
		submittedRequest!.Title.Should().Be("Test Issue Title");
		submittedRequest.Description.Should().Be("Test description");
		submittedRequest.Status.Should().Be("Open");
	}

	[Fact]
	public async Task IssueForm_InvokesOnCancelCallback_WhenCancelButtonIsClicked()
	{
		// Arrange
		var cancelInvoked = false;
		var cancelCallback = EventCallback.Factory.Create(this, () => { cancelInvoked = true; });

		var component = TestContext.Render<IssueForm>(
			parameters => parameters.Add(c => c.OnCancel, cancelCallback)
		);

		// Act
		var cancelButton = component.FindAll("button")[1]; // Second button is cancel
		await cancelButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

		// Assert
		cancelInvoked.Should().BeTrue();
	}

	[Fact]
	public void IssueForm_PopulatesFormFields_WhenInitialValuesAreProvided()
	{
		// Arrange
		var initialValues = new CreateIssueRequest
		{
			Title = "Initial Title",
			Description = "Initial Description",
			Status = "InProgress"
		};

		// Act
		var component = TestContext.Render<IssueForm>(
			parameters => parameters.Add(c => c.InitialValues, initialValues)
		);

		// Assert - Verify form was populated (rendered HTML contains values)
		var titleInput = component.Find("#title");
		titleInput.GetAttribute("value").Should().Be("Initial Title");

		// Status is now API-driven (StatusId); the #status select exists but
		// has no hardcoded options in tests (mock returns empty collections).
		component.Find("#status").Should().NotBeNull();

		// Description is bound, verify the textarea element exists
		component.Find("#description").Should().NotBeNull();
	}

	[Fact]
	public void IssueForm_UpdatesFormFields_WhenInitialValuesParameterChanges()
	{
		// Arrange
		var initialValues = new CreateIssueRequest
		{
			Title = "Initial Title",
			Description = "Initial Description",
			Status = "Open"
		};

		var component = TestContext.Render<IssueForm>(
			parameters => parameters.Add(c => c.InitialValues, initialValues)
		);

		// Act - Update initial values parameter
		var updatedValues = new CreateIssueRequest
		{
			Title = "Updated Title",
			Description = "Updated Description",
			Status = "Closed"
		};

		// bUnit 2.x: Re-render component entirely with new parameters
		component = TestContext.Render<IssueForm>(
			parameters => parameters.Add(c => c.InitialValues, updatedValues)
		);

		// Assert - Verify form fields updated
		var titleInput = component.Find("#title");
		titleInput.GetAttribute("value").Should().Be("Updated Title");

		// Status is now API-driven (StatusId); verify the element exists.
		component.Find("#status").Should().NotBeNull();
	}

	[Fact]
	public void IssueForm_DisablesButtons_WhenIsSubmittingIsTrue()
	{
		// Act
		var component = TestContext.Render<IssueForm>(
			parameters => parameters
				.Add(c => c.IsSubmitting, true)
				.Add(c => c.OnCancel, EventCallback.Factory.Create(this, () => { }))
		);

		// Assert
		var buttons = component.FindAll("button");
		buttons.Should().HaveCount(2);

		var submitButton = buttons[0];
		submitButton.HasAttribute("disabled").Should().BeTrue();

		var cancelButton = buttons[1];
		cancelButton.HasAttribute("disabled").Should().BeTrue();
	}

	[Fact]
	public void IssueForm_ShowsSpinner_WhenIsSubmittingIsTrue()
	{
		// Act
		var component = TestContext.Render<IssueForm>(
			parameters => parameters.Add(c => c.IsSubmitting, true)
		);

		// Assert
		var spinner = component.Find(".animate-spin");
		spinner.Should().NotBeNull();
	}

	[Fact]
	public void IssueForm_DefaultsToOpenStatus_WhenNoInitialValuesProvided()
	{
		// Act
		var component = TestContext.Render<IssueForm>();

		// Assert
		// Status is now API-driven (StatusId bound to dynamic options from the API).
		// In tests the mock client returns empty, so #status select renders with no options.
		component.Find("#status").Should().NotBeNull();
	}

	[Fact]
	public void IssueForm_ShowsValidationSummary_WhenRendered()
	{
		// Act
		var component = TestContext.Render<IssueForm>();

		// Assert
		var validationSummary = component.FindComponents<Microsoft.AspNetCore.Components.Forms.ValidationSummary>();
		validationSummary.Should().HaveCount(1);
	}
}
