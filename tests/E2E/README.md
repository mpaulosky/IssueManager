# E2E Tests with Playwright

## Overview

This project contains end-to-end tests for IssueManager using Playwright for .NET. Tests are written in C# and xUnit, and cover critical user workflows in a real browser environment.

## Test Coverage

### 1. Issue Creation Tests (8 tests)
- Navigate to create page
- Create issue with valid data
- Validation for empty title
- Validation for title too short
- Create with minimal data (no description)
- Cancel creation
- Create with different statuses (Open, InProgress, Closed)

### 2. Issue List Tests (6 tests)
- Navigate to issue list
- View issue list
- Click create button
- Filter by status
- Search for issues
- Click issue to view details

### 3. Issue Detail Tests (4 tests)
- View issue details
- Navigate to edit from detail page
- Navigate back to list
- View issue metadata (timestamps, status)

### 4. Issue Status Update Tests (3 tests)
- Update status from detail page
- Close issue from detail page
- Edit issue from detail page

### 5. Navigation Tests (4 tests)
- Navigate from home to issue list
- Complete create flow (list → create → detail → list)
- Navigate to home from any page
- Navigate between issue details

### 6. Error Handling Tests (5 tests)
- Validation summary for multiple errors
- Cannot submit while submitting
- Error for non-existent issue
- Field-level validation errors
- Recover from validation error

**Total: 30 E2E tests**

## Prerequisites

1. **Playwright Browsers** — Install Playwright browsers:
   ```bash
   pwsh bin/Debug/net10.0/playwright.ps1 install
   ```
   Or use the dotnet CLI:
   ```bash
   dotnet tool update --global Microsoft.Playwright.CLI
   playwright install chromium
   ```

2. **Running Application** — The application must be running before tests execute:
   ```bash
   cd src/AppHost
   dotnet run
   ```
   Default base URL: `http://localhost:5000`

3. **Environment Variable** (optional) — Override base URL:
   ```bash
   $env:E2E_BASE_URL = "http://localhost:8080"
   ```

## Running Tests

### Run all E2E tests:
```bash
dotnet test
```

### Run specific test class:
```bash
dotnet test --filter "FullyQualifiedName~IssueCreationTests"
```

### Run with verbose output:
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run in headed mode (see browser):
Set `Headless: false` in `PlaywrightFixture.cs` or modify the launch options.

## Test Architecture

### Page Object Model (POM)

Tests use page objects to encapsulate page interactions:

- **HomePage** — Home page interactions
- **IssueFormPage** — Issue creation/editing form
- **IssueListPage** — Issue list, filtering, searching
- **IssueDetailPage** — Issue detail view, status updates

### Fixtures

- **PlaywrightFixture** — Manages browser lifecycle, page creation, base URL configuration

### Test Organization

- `/Fixtures` — Test fixtures (browser management)
- `/PageObjects` — Page object models
- `/Tests` — Test classes organized by workflow

## Best Practices

1. **Test Isolation** — Each test is independent, uses unique timestamps for test data
2. **Async/Await** — All Playwright operations are async
3. **Explicit Waits** — Use `WaitForURLAsync` and `IsVisibleAsync` instead of `Thread.Sleep`
4. **Declarative Tests** — Tests read like user stories
5. **Maintainability** — Page objects reduce duplication, centralize selectors

## Configuration

- **Base URL:** Set via `E2E_BASE_URL` environment variable or `appsettings.json`
- **Browser:** Chromium (default), can be changed in `PlaywrightFixture`
- **Headless:** `true` by default (CI-friendly), set to `false` for debugging
- **Viewport:** 1920x1080 (desktop resolution)

## CI/CD Integration

Tests are designed to run in CI/CD pipelines:

- Headless mode by default
- No external dependencies (except running app)
- Fast execution (<5 seconds per test)
- Clear failure messages

## Troubleshooting

### Playwright not installed
```bash
playwright install
```

### Application not running
Ensure the Aspire AppHost is running:
```bash
cd src/AppHost
dotnet run
```

### Port conflicts
Change the base URL in `appsettings.json` or set `E2E_BASE_URL` environment variable.

### Test failures due to timing
Increase timeouts in `PlaywrightFixture` or use explicit waits.

## Future Enhancements

- Screenshot capture on failure
- Video recording for debugging
- Parallel test execution
- Cross-browser testing (Firefox, WebKit)
- Mobile viewport testing
- Accessibility testing (ARIA roles, keyboard navigation)
