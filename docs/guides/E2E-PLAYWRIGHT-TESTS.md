# E2E Testing Guide (Playwright)

## Overview

End-to-end (E2E) tests verify complete user workflows in a real browser. They test the entire application stack from UI to API to database.

**When to use E2E tests:**
- Testing critical user journeys (create issue, edit issue, etc.)
- Testing multi-step workflows (login → create → edit → delete)
- Testing UI interactions (clicks, form submissions, navigation)
- Testing browser-specific behavior (responsive design, accessibility)
- Smoke testing after deployment

**Framework used:**
- **Playwright** — Browser automation for .NET

## Setup

### Prerequisites
- .NET 10 SDK
- Playwright browsers installed

### Install Playwright
```bash
# Install Playwright package
dotnet add tests/E2E package Microsoft.Playwright

# Install browsers (Chrome, Firefox, WebKit)
pwsh bin/Debug/net10.0/playwright.ps1 install
```

### Create an E2E Test File

1. Add test file to `tests/E2E/`
2. Reference Playwright via GlobalUsings:
   ```csharp
   // tests/E2E/GlobalUsings.cs
   global using Xunit;
   global using Microsoft.Playwright;
   global using FluentAssertions;
   ```

3. Create test class:
   ```csharp
   namespace IssueManager.Tests.E2E;

   /// <summary>
   /// E2E tests for issue creation workflow.
   /// </summary>
   public class IssueCreationTests : IAsyncLifetime
   {
       private IPlaywright _playwright = null!;
       private IBrowser _browser = null!;
       private IPage _page = null!;

       public async Task InitializeAsync()
       {
           _playwright = await Playwright.CreateAsync();
           _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
           {
               Headless = true, // Set to false for debugging
               SlowMo = 100 // Slow down actions for debugging
           });
           _page = await _browser.NewPageAsync();
       }

       public async Task DisposeAsync()
       {
           await _page.CloseAsync();
           await _browser.CloseAsync();
           _playwright.Dispose();
       }
   }
   ```

## Example: Testing Issue Creation

```csharp
[Fact]
public async Task CreateIssue_ValidData_CreatesIssueSuccessfully()
{
    // Given — Navigate to create issue page
    await _page.GotoAsync("https://localhost:5001/issues/create");

    // When — Fill in form
    await _page.FillAsync("#title", "E2E Test Issue");
    await _page.FillAsync("#description", "This issue was created by an E2E test.");
    await _page.SelectOptionAsync("#status", "Open");
    await _page.ClickAsync("button[type='submit']");

    // Then — Verify success
    await _page.WaitForURLAsync("**/issues");
    var issueList = await _page.TextContentAsync("body");
    issueList.Should().Contain("E2E Test Issue");
}
```

## Playwright Basics

### Navigate to Pages
```csharp
// Navigate to URL
await _page.GotoAsync("https://localhost:5001/issues");

// Wait for navigation
await _page.WaitForURLAsync("**/issues");

// Go back/forward
await _page.GoBackAsync();
await _page.GoForwardAsync();

// Reload page
await _page.ReloadAsync();
```

### Find Elements
```csharp
// By CSS selector
var titleInput = await _page.QuerySelectorAsync("#title");
var submitButton = await _page.QuerySelectorAsync("button[type='submit']");
var firstIssue = await _page.QuerySelectorAsync(".issue-item:first-child");

// Find multiple elements
var issues = await _page.QuerySelectorAllAsync(".issue-item");
issues.Count().Should().BeGreaterThan(0);

// By text content
var createButton = await _page.GetByTextAsync("Create Issue");
```

### Interact with Elements

#### Fill Input Fields
```csharp
// Text input
await _page.FillAsync("#title", "My Issue Title");

// Textarea
await _page.FillAsync("#description", "Long description...");

// Clear and fill
await _page.FillAsync("#title", ""); // Clear
await _page.FillAsync("#title", "New Title");
```

#### Click Elements
```csharp
// Click button
await _page.ClickAsync("button[type='submit']");

// Click link
await _page.ClickAsync("a[href='/issues/123']");

// Double-click
await _page.DblClickAsync(".editable-field");

// Right-click
await _page.ClickAsync(".context-menu-trigger", new PageClickOptions { Button = MouseButton.Right });
```

#### Select Dropdowns
```csharp
// Select by value
await _page.SelectOptionAsync("#status", "Open");

// Select by label
await _page.SelectOptionAsync("#status", new SelectOptionValue { Label = "Open" });

// Select by index
await _page.SelectOptionAsync("#status", new SelectOptionValue { Index = 0 });
```

#### Checkboxes and Radios
```csharp
// Check checkbox
await _page.CheckAsync("#agree-to-terms");

// Uncheck checkbox
await _page.UncheckAsync("#agree-to-terms");

// Radio button
await _page.CheckAsync("#priority-high");
```

### Wait for Elements
```csharp
// Wait for element to be visible
await _page.WaitForSelectorAsync(".success-message");

// Wait for element to be hidden
await _page.WaitForSelectorAsync(".loading-spinner", new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden });

// Wait for URL change
await _page.WaitForURLAsync("**/issues");

// Wait for load state
await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

// Wait with timeout
await _page.WaitForSelectorAsync(".slow-element", new PageWaitForSelectorOptions { Timeout = 10000 });
```

### Assertions
```csharp
// Text content
var text = await _page.TextContentAsync("h1");
text.Should().Be("Issue Manager");

// Attribute value
var value = await _page.GetAttributeAsync("#title", "value");
value.Should().Be("Expected Title");

// Element visibility
var isVisible = await _page.IsVisibleAsync(".success-message");
isVisible.Should().BeTrue();

// Element enabled state
var isEnabled = await _page.IsEnabledAsync("button[type='submit']");
isEnabled.Should().BeTrue();

// URL
_page.Url.Should().Contain("/issues");
```

## Common E2E Test Patterns

### Given-When-Then Pattern
```csharp
[Fact]
public async Task EditIssue_ValidData_UpdatesIssueSuccessfully()
{
    // Given — Create an issue and navigate to edit page
    var issueId = await CreateTestIssue("Original Title");
    await _page.GotoAsync($"https://localhost:5001/issues/{issueId}/edit");

    // When — Update the title
    await _page.FillAsync("#title", "Updated Title");
    await _page.ClickAsync("button[type='submit']");

    // Then — Verify update
    await _page.WaitForURLAsync($"**/issues/{issueId}");
    var title = await _page.TextContentAsync("h1");
    title.Should().Contain("Updated Title");
}
```

### Page Object Pattern
Encapsulate page interactions in classes:

```csharp
public class IssueFormPage
{
    private readonly IPage _page;

    public IssueFormPage(IPage page)
    {
        _page = page;
    }

    public async Task NavigateAsync()
    {
        await _page.GotoAsync("https://localhost:5001/issues/create");
    }

    public async Task FillFormAsync(string title, string description, string status = "Open")
    {
        await _page.FillAsync("#title", title);
        await _page.FillAsync("#description", description);
        await _page.SelectOptionAsync("#status", status);
    }

    public async Task SubmitAsync()
    {
        await _page.ClickAsync("button[type='submit']");
        await _page.WaitForURLAsync("**/issues");
    }
}

// Usage in test
[Fact]
public async Task CreateIssue_ValidData_CreatesIssueSuccessfully()
{
    var formPage = new IssueFormPage(_page);
    await formPage.NavigateAsync();
    await formPage.FillFormAsync("Test Issue", "Test Description");
    await formPage.SubmitAsync();

    var issueList = await _page.TextContentAsync("body");
    issueList.Should().Contain("Test Issue");
}
```

### Test Data Builders
```csharp
public class IssueDataBuilder
{
    private string _title = "Default Title";
    private string _description = "Default Description";
    private string _status = "Open";

    public IssueDataBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public IssueDataBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public IssueDataBuilder WithStatus(string status)
    {
        _status = status;
        return this;
    }

    public (string Title, string Description, string Status) Build()
    {
        return (_title, _description, _status);
    }
}

// Usage
var issueData = new IssueDataBuilder()
    .WithTitle("Critical Bug")
    .WithDescription("Production issue")
    .Build();
```

## Testing Workflows

### Multi-Step Workflow
```csharp
[Fact]
public async Task IssueLifecycle_CreateEditDelete_WorksEndToEnd()
{
    // Step 1: Create issue
    await _page.GotoAsync("https://localhost:5001/issues/create");
    await _page.FillAsync("#title", "Lifecycle Test Issue");
    await _page.ClickAsync("button[type='submit']");
    await _page.WaitForURLAsync("**/issues");

    // Step 2: Edit issue
    await _page.ClickAsync("a:has-text('Lifecycle Test Issue')");
    await _page.ClickAsync("button:has-text('Edit')");
    await _page.FillAsync("#title", "Updated Lifecycle Test Issue");
    await _page.ClickAsync("button[type='submit']");

    // Step 3: Verify update
    var title = await _page.TextContentAsync("h1");
    title.Should().Contain("Updated Lifecycle Test Issue");

    // Step 4: Delete issue
    await _page.ClickAsync("button:has-text('Delete')");
    await _page.ClickAsync("button:has-text('Confirm')");
    await _page.WaitForURLAsync("**/issues");

    // Step 5: Verify deletion
    var issueList = await _page.TextContentAsync("body");
    issueList.Should().NotContain("Updated Lifecycle Test Issue");
}
```

### Testing Search and Filters
```csharp
[Fact]
public async Task IssueList_FilterByStatus_ShowsCorrectIssues()
{
    // Given — Navigate to issue list
    await _page.GotoAsync("https://localhost:5001/issues");

    // When — Filter by "Open" status
    await _page.SelectOptionAsync("#status-filter", "Open");
    await _page.ClickAsync("button:has-text('Apply Filters')");

    // Then — Only open issues are shown
    var issues = await _page.QuerySelectorAllAsync(".issue-item");
    foreach (var issue in issues)
    {
        var status = await issue.GetAttributeAsync("data-status");
        status.Should().Be("Open");
    }
}
```

## Configuration

### Headless vs. Headed Mode
```csharp
// Headless (for CI)
_browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
{
    Headless = true
});

// Headed (for local debugging)
_browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
{
    Headless = false,
    SlowMo = 100 // Slow down for observation
});
```

### Browser Selection
```csharp
// Chromium (default)
_browser = await _playwright.Chromium.LaunchAsync();

// Firefox
_browser = await _playwright.Firefox.LaunchAsync();

// WebKit (Safari)
_browser = await _playwright.Webkit.LaunchAsync();
```

### Viewport and Device Emulation
```csharp
// Desktop viewport
_page = await _browser.NewPageAsync(new BrowserNewPageOptions
{
    ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
});

// Mobile viewport
_page = await _browser.NewPageAsync(new BrowserNewPageOptions
{
    ViewportSize = new ViewportSize { Width = 375, Height = 667 }
});

// Device emulation
var iPhone = _playwright.Devices["iPhone 12"];
_page = await _browser.NewPageAsync(iPhone);
```

## Best Practices

### ✅ Do
- **Test critical user journeys** — Focus on high-value workflows
- **Use descriptive selectors** — Prefer IDs or data attributes over brittle CSS
- **Wait for elements** — Don't rely on fixed timeouts
- **Use Page Object pattern** — Encapsulate page interactions
- **Run headless in CI** — Faster, more reliable
- **Take screenshots on failure** — Aid debugging
- **Test one workflow per test** — Focused, clear failures

### ❌ Don't
- **Test every edge case** — That's for unit/integration tests
- **Use hardcoded sleeps** — Use Playwright's waiting strategies
- **Test implementation details** — Focus on user-visible behavior
- **Share state between tests** — Each test should be independent
- **Run E2E tests too frequently** — They're slow; run on PR/deploy

## Common Mistakes

### ❌ Using Thread.Sleep
```csharp
// Bad — Brittle and slow
await _page.ClickAsync("button");
await Task.Delay(2000); // Hope element appears
var text = await _page.TextContentAsync(".result");
```

### ✅ Wait for Element
```csharp
// Good — Wait for element to be visible
await _page.ClickAsync("button");
await _page.WaitForSelectorAsync(".result");
var text = await _page.TextContentAsync(".result");
```

### ❌ Brittle Selectors
```csharp
// Bad — Fragile, breaks with CSS changes
var button = await _page.QuerySelectorAsync("div > div > button.btn.btn-primary");
```

### ✅ Semantic Selectors
```csharp
// Good — Uses data attributes or IDs
var button = await _page.QuerySelectorAsync("#submit-button");
var button = await _page.QuerySelectorAsync("[data-testid='submit-button']");
```

## Debugging E2E Failures

### Run in Headed Mode
```csharp
Headless = false, // See browser actions
SlowMo = 500 // Slow down by 500ms per action
```

### Take Screenshots
```csharp
// On failure
await _page.ScreenshotAsync(new PageScreenshotOptions
{
    Path = "failure.png",
    FullPage = true
});
```

### Video Recording
```csharp
var context = await _browser.NewContextAsync(new BrowserNewContextOptions
{
    RecordVideoDir = "videos/"
});
_page = await context.NewPageAsync();

// After test
await context.CloseAsync(); // Finalizes video
```

### Playwright Inspector
```bash
# Set environment variable
$env:PWDEBUG=1
dotnet test tests/E2E --filter "FullyQualifiedName~MyTest"
```

### Console Logs
```csharp
_page.Console += (_, msg) => Console.WriteLine($"Browser console: {msg.Text}");
```

## Running E2E Tests

### Prerequisites
Ensure the application is running:
```bash
# Terminal 1: Run the app
dotnet run --project AppHost

# Terminal 2: Run E2E tests
dotnet test tests/E2E
```

### Run Tests
```bash
# Run all E2E tests
dotnet test tests/E2E

# Run specific test
dotnet test --filter "FullyQualifiedName~IssueCreationTests"

# Run in headed mode (set in test code)
dotnet test tests/E2E
```

### CI/CD Integration
```yaml
# GitHub Actions example
- name: Run E2E Tests
  run: |
    dotnet run --project AppHost &
    sleep 10
    dotnet test tests/E2E --logger "trx;LogFileName=e2e-results.trx"
```

## Performance Tuning

### Parallel Execution
xUnit runs test classes in parallel, but each browser instance is isolated.

### Optimize Waits
```csharp
// Use specific waits instead of WaitForLoadState
await _page.WaitForSelectorAsync(".content");
```

### Reuse Browser Context
Share browser context across tests in a class (advanced):
```csharp
private static IBrowserContext _context = null!;

public async Task InitializeAsync()
{
    _context ??= await _browser.NewContextAsync();
    _page = await _context.NewPageAsync();
}
```

## See Also

- [Testing Strategy](../TESTING.md) — Overall test philosophy
- [Integration Testing Guide](INTEGRATION-TESTS.md) — Testing handlers
- [Playwright .NET Documentation](https://playwright.dev/dotnet/)

---

**E2E tests directory:** `tests/E2E/`
