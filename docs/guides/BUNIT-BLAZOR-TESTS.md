# Blazor Component Testing Guide (bUnit)

## Overview

bUnit is a testing library for Blazor components. It renders components in a test context and allows you to interact with the rendered output, test parameters, event callbacks, and component lifecycle.

**When to use bUnit:**
- Testing component rendering
- Testing component parameters and cascading parameters
- Testing event callbacks (button clicks, form submissions)
- Testing component state and lifecycle
- Testing forms and validation

**Frameworks used:**
- **bUnit** — Blazor component testing
- **xUnit** — Test runner
- **FluentAssertions** — Readable assertions

## Setup

### Create a Blazor Component Test File

1. Add test file to `tests/BlazorTests/Components/`
2. Inherit from `ComponentTestBase` (our custom base class)
3. Reference frameworks via GlobalUsings:
   ```csharp
   // tests/BlazorTests/GlobalUsings.cs
   global using Bunit;
   global using FluentAssertions;
   global using Xunit;
   global using IssueManager.Shared.Domain;
   global using IssueManager.Web.Components;
   ```

### Base Class
We provide `ComponentTestBase` for common setup:

```csharp
// tests/BlazorTests/Fixtures/ComponentTestBase.cs
public abstract class ComponentTestBase : IDisposable
{
    protected TestContext TestContext { get; }

    protected ComponentTestBase()
    {
        TestContext = new TestContext();
        // Add common test services here if needed
    }

    public void Dispose()
    {
        TestContext?.Dispose();
        GC.SuppressFinalize(this);
    }
}
```

## Example: Testing IssueForm Component

**Real example from the codebase:** [`tests/BlazorTests/Components/IssueFormTests.cs`](../../tests/BlazorTests/Components/IssueFormTests.cs)

```csharp
namespace IssueManager.Tests.BlazorTests.Components;

/// <summary>
/// Tests for the IssueForm Blazor component.
/// </summary>
public class IssueFormTests : ComponentTestBase
{
    [Fact]
    public void IssueForm_RendersCorrectly_WhenInitialized()
    {
        // Act
        var component = TestContext.RenderComponent<IssueForm>();

        // Assert
        component.Should().NotBeNull();
        component.Find("form").Should().NotBeNull();
        component.Find("#title").Should().NotBeNull();
        component.Find("#description").Should().NotBeNull();
        component.Find("button[type='submit']").Should().NotBeNull();
    }
}
```

## Basic Component Rendering

### Render a Component
```csharp
[Fact]
public void MyComponent_RendersCorrectly()
{
    // Act
    var component = TestContext.RenderComponent<MyComponent>();

    // Assert
    component.Should().NotBeNull();
    component.Markup.Should().Contain("Expected HTML content");
}
```

### Find Elements
```csharp
// Find by CSS selector
var button = component.Find("button");
var titleInput = component.Find("#title");
var submitButton = component.Find("button[type='submit']");

// Find multiple elements
var buttons = component.FindAll("button");
buttons.Should().HaveCount(2);

// Assert element exists
component.Find("form").Should().NotBeNull();
```

### Assert Element Content
```csharp
var submitButton = component.Find("button[type='submit']");
submitButton.TextContent.Should().Contain("Create Issue");

var titleInput = component.Find("#title");
titleInput.GetAttribute("value").Should().Be("Expected Title");
```

## Testing Component Parameters

### Pass Parameters
```csharp
[Fact]
public void IssueForm_ShowsCreateButtonText_WhenIsEditModeIsFalse()
{
    // Act
    var component = TestContext.RenderComponent<IssueForm>(
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
    var component = TestContext.RenderComponent<IssueForm>(
        parameters => parameters.Add(c => c.IsEditMode, true)
    );

    // Assert
    var submitButton = component.Find("button[type='submit']");
    submitButton.TextContent.Should().Contain("Update Issue");
}
```

### Update Parameters After Rendering
```csharp
[Fact]
public void IssueForm_UpdatesFormFields_WhenInitialValuesParameterChanges()
{
    // Arrange
    var initialValues = new CreateIssueRequest { Title = "Initial Title" };
    var component = TestContext.RenderComponent<IssueForm>(
        parameters => parameters.Add(c => c.InitialValues, initialValues)
    );

    // Act — Update parameter
    var updatedValues = new CreateIssueRequest { Title = "Updated Title" };
    component.SetParametersAndRender(
        parameters => parameters.Add(c => c.InitialValues, updatedValues)
    );

    // Assert
    var titleInput = component.Find("#title");
    titleInput.GetAttribute("value").Should().Be("Updated Title");
}
```

## Testing Event Callbacks

### OnSubmit Callback
```csharp
[Fact]
public async Task IssueForm_InvokesOnSubmitCallback_WhenFormIsSubmittedWithValidData()
{
    // Arrange
    CreateIssueRequest? submittedRequest = null;
    var submitCallback = EventCallback.Factory.Create<CreateIssueRequest>(
        this,
        request => { submittedRequest = request; }
    );

    var component = TestContext.RenderComponent<IssueForm>(
        parameters => parameters.Add(c => c.OnSubmit, submitCallback)
    );

    // Act — Fill in form fields
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
}
```

### OnCancel Callback
```csharp
[Fact]
public async Task IssueForm_InvokesOnCancelCallback_WhenCancelButtonIsClicked()
{
    // Arrange
    var cancelInvoked = false;
    var cancelCallback = EventCallback.Factory.Create(this, () => { cancelInvoked = true; });

    var component = TestContext.RenderComponent<IssueForm>(
        parameters => parameters.Add(c => c.OnCancel, cancelCallback)
    );

    // Act
    var cancelButton = component.FindAll("button")[1];
    await cancelButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

    // Assert
    cancelInvoked.Should().BeTrue();
}
```

## Testing Forms

### Interacting with Form Fields
```csharp
// Text input
var titleInput = component.Find("#title");
titleInput.Change("New Title");

// Textarea
var descriptionInput = component.Find("#description");
descriptionInput.Change("New Description");

// Select dropdown
var statusSelect = component.Find("#status");
statusSelect.Change(IssueStatus.InProgress.ToString());

// Checkbox
var checkbox = component.Find("#agreeToTerms");
checkbox.Change(true);
```

### Submitting Forms
```csharp
var form = component.Find("form");
await form.SubmitAsync();
```

### Testing Validation
```csharp
[Fact]
public void IssueForm_ShowsValidationSummary_WhenRendered()
{
    // Act
    var component = TestContext.RenderComponent<IssueForm>();

    // Assert
    var validationSummary = component.FindComponents<Microsoft.AspNetCore.Components.Forms.ValidationSummary>();
    validationSummary.Should().HaveCount(1);
}
```

## Testing Component State

### Conditional Rendering
```csharp
[Fact]
public void IssueForm_ShowsCancelButton_WhenOnCancelCallbackIsDefined()
{
    // Arrange
    var cancelCallback = EventCallback.Factory.Create(this, () => { });

    // Act
    var component = TestContext.RenderComponent<IssueForm>(
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
    var component = TestContext.RenderComponent<IssueForm>();

    // Assert
    var buttons = component.FindAll("button");
    buttons.Should().HaveCount(1); // Only submit button
}
```

### Loading/Disabled State
```csharp
[Fact]
public void IssueForm_DisablesButtons_WhenIsSubmittingIsTrue()
{
    // Act
    var component = TestContext.RenderComponent<IssueForm>(
        parameters => parameters
            .Add(c => c.IsSubmitting, true)
            .Add(c => c.OnCancel, EventCallback.Factory.Create(this, () => { }))
    );

    // Assert
    var buttons = component.FindAll("button");
    buttons[0].HasAttribute("disabled").Should().BeTrue();
    buttons[1].HasAttribute("disabled").Should().BeTrue();
}

[Fact]
public void IssueForm_ShowsSpinner_WhenIsSubmittingIsTrue()
{
    // Act
    var component = TestContext.RenderComponent<IssueForm>(
        parameters => parameters.Add(c => c.IsSubmitting, true)
    );

    // Assert
    var spinner = component.Find(".spinner-border");
    spinner.Should().NotBeNull();
}
```

## Mocking Services

If a component depends on services (e.g., `IIssueService`), mock them:

```csharp
public class MyComponentTests : ComponentTestBase
{
    [Fact]
    public void MyComponent_CallsService_RendersData()
    {
        // Arrange
        var mockService = Substitute.For<IIssueService>();
        mockService.GetIssuesAsync().Returns(new List<Issue>
        {
            new Issue { Id = "1", Title = "Test Issue" }
        });

        TestContext.Services.AddSingleton(mockService);

        // Act
        var component = TestContext.RenderComponent<MyComponent>();

        // Assert
        component.Markup.Should().Contain("Test Issue");
    }
}
```

## Testing Component Lifecycle

### OnInitialized / OnInitializedAsync
```csharp
[Fact]
public async Task MyComponent_LoadsData_OnInitialized()
{
    // Arrange
    var mockService = Substitute.For<IIssueService>();
    mockService.GetIssuesAsync().Returns(Task.FromResult(new List<Issue> { /* ... */ }));
    TestContext.Services.AddSingleton(mockService);

    // Act
    var component = TestContext.RenderComponent<MyComponent>();

    // Wait for async initialization
    await Task.Delay(100);

    // Assert
    await mockService.Received(1).GetIssuesAsync();
}
```

### OnParametersSet / OnParametersSetAsync
Use `SetParametersAndRender` to trigger lifecycle:

```csharp
[Fact]
public void MyComponent_ReactsToParameterChange()
{
    // Arrange
    var component = TestContext.RenderComponent<MyComponent>(
        parameters => parameters.Add(c => c.IssueId, "123")
    );

    // Act — Change parameter
    component.SetParametersAndRender(
        parameters => parameters.Add(c => c.IssueId, "456")
    );

    // Assert
    component.Markup.Should().Contain("456");
}
```

## Best Practices

### ✅ Do
- **Inherit from ComponentTestBase** — Reuse setup logic
- **Test one thing per test** — Rendering, parameters, callbacks separately
- **Use descriptive names** — `IssueForm_ShowsUpdateButton_WhenIsEditModeIsTrue`
- **Test observable behavior** — What users see, not internal state
- **Use async/await for callbacks** — Forms and events are often async
- **Clean up with IDisposable** — TestContext.Dispose()

### ❌ Don't
- **Test implementation details** — Focus on rendered output
- **Use Thread.Sleep** — Use async/await or bUnit's WaitFor
- **Share TestContext between tests** — Each test should have its own
- **Test Blazor framework internals** — Test your component, not Blazor

## Common Mistakes

### ❌ Not Awaiting Async Events
```csharp
// Bad — Missing await
var button = component.Find("button");
button.ClickAsync(new MouseEventArgs()); // Fire and forget
```

### ✅ Always Await Async Interactions
```csharp
// Good — Await the event
var button = component.Find("button");
await button.ClickAsync(new MouseEventArgs());
```

### ❌ Testing Too Many Things
```csharp
// Bad — Tests rendering, parameters, and callbacks in one test
[Fact]
public void IssueForm_Everything_Works() { /* ... */ }
```

### ✅ Split Into Focused Tests
```csharp
[Fact]
public void IssueForm_RendersCorrectly() { /* ... */ }

[Fact]
public void IssueForm_ShowsUpdateButton_WhenIsEditModeIsTrue() { /* ... */ }

[Fact]
public async Task IssueForm_InvokesOnSubmitCallback() { /* ... */ }
```

## Common Patterns

### Arrange-Act-Assert with Components
```csharp
[Fact]
public void MyComponent_Scenario_ExpectedOutcome()
{
    // Arrange — Set up services, parameters
    var mockService = Substitute.For<IMyService>();
    TestContext.Services.AddSingleton(mockService);

    // Act — Render component
    var component = TestContext.RenderComponent<MyComponent>(
        parameters => parameters.Add(c => c.MyParam, "value")
    );

    // Assert — Verify rendered output
    component.Markup.Should().Contain("Expected Content");
}
```

### Testing Conditional Rendering
```csharp
// Test presence
var element = component.Find(".my-element");
element.Should().NotBeNull();

// Test absence (use FindAll)
var elements = component.FindAll(".hidden-element");
elements.Should().BeEmpty();
```

## Debugging Failed Tests

1. **Inspect `component.Markup`** — See the rendered HTML
   ```csharp
   Console.WriteLine(component.Markup);
   ```

2. **Use bUnit's MarkupMatches** — Compare exact HTML
   ```csharp
   component.MarkupMatches("<div>Expected HTML</div>");
   ```

3. **Set breakpoints** — In test or component code
4. **Run test in isolation** — Use Test Explorer or `dotnet test --filter`
5. **Check async timing** — Add `await Task.Delay(100)` if needed (last resort)

## Running Blazor Component Tests

```bash
# Run all Blazor tests
dotnet test tests/BlazorTests

# Run specific test
dotnet test --filter "FullyQualifiedName~IssueFormTests"

# Run in watch mode
dotnet watch test --project tests/BlazorTests
```

## See Also

- [Testing Strategy](../TESTING.md) — Overall test philosophy
- [Unit Testing Guide](UNIT-TESTS.md) — Testing validators and services
- [bUnit Documentation](https://bunit.dev/)
- [FluentAssertions Documentation](https://fluentassertions.com/)

---

**Real examples in the codebase:**
- [`tests/BlazorTests/Components/IssueFormTests.cs`](../../tests/BlazorTests/Components/IssueFormTests.cs)
- [`tests/BlazorTests/Fixtures/ComponentTestBase.cs`](../../tests/BlazorTests/Fixtures/ComponentTestBase.cs)
