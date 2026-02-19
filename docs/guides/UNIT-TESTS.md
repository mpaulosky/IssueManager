# Unit Testing Guide

## Overview

Unit tests verify individual components in isolation. They should be fast (<100ms), focused (test one thing), and have no external dependencies (no database, API calls, or file I/O).

**When to use unit tests:**
- Testing validators (FluentValidation rules)
- Testing domain models (business logic)
- Testing services with mocked dependencies
- Testing pure functions and calculations

**Frameworks used:**
- **xUnit** — Test runner
- **FluentAssertions** — Readable assertions
- **FluentValidation** — Validation library
- **NSubstitute** — Mocking (for future service tests)

## Setup

### Create a Unit Test File

1. Add test file to `tests/Unit/` (or appropriate subfolder like `Validators/`, `Domain/`)
2. Reference frameworks via GlobalUsings:
   ```csharp
   // tests/Unit/GlobalUsings.cs
   global using Xunit;
   global using FluentAssertions;
   global using IssueManager.Shared.Domain;
   global using IssueManager.Shared.Validators;
   ```

3. Create test class:
   ```csharp
   namespace IssueManager.Tests.Unit.Validators;

   /// <summary>
   /// Unit tests for <see cref="CreateIssueValidator"/>.
   /// </summary>
   public class CreateIssueValidatorTests
   {
       // Tests go here
   }
   ```

## Example: Testing a Validator

**Real example from the codebase:** [`tests/Unit/Validators/CreateIssueValidatorTests.cs`](../../tests/Unit/Validators/CreateIssueValidatorTests.cs)

```csharp
[Fact]
public void CreateIssueValidator_ValidCommand_ReturnsNoErrors()
{
    // Arrange
    var validator = new CreateIssueValidator();
    var command = new CreateIssueCommand
    {
        Title = "Fix login bug",
        Description = "SSO authentication is broken for external users"
    };

    // Act
    var result = validator.Validate(command);

    // Assert
    result.IsValid.Should().BeTrue();
    result.Errors.Should().BeEmpty();
}

[Fact]
public void CreateIssueValidator_EmptyTitle_ReturnsValidationError()
{
    // Arrange
    var validator = new CreateIssueValidator();
    var command = new CreateIssueCommand { Title = "", Description = "Some description" };

    // Act
    var result = validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().HaveCountGreaterOrEqualTo(1);
    result.Errors.Should().Contain(e => e.PropertyName == "Title" && e.ErrorMessage.Contains("required"));
}
```

### Key Points
- **Arrange:** Create validator and command/query
- **Act:** Call `validator.Validate(command)`
- **Assert:** Check `IsValid`, `Errors` collection

## Test Structure

All unit tests follow **Arrange-Act-Assert** (AAA):

```csharp
[Fact]
public void MethodUnderTest_Scenario_ExpectedOutcome()
{
    // Arrange — Set up test data and dependencies
    var validator = new MyValidator();
    var input = new MyCommand { /* ... */ };

    // Act — Execute the code under test
    var result = validator.Validate(input);

    // Assert — Verify the outcome
    result.IsValid.Should().BeTrue();
}
```

## FluentAssertions Basics

FluentAssertions provides readable, expressive assertions:

```csharp
// Boolean assertions
result.IsValid.Should().BeTrue();
result.IsValid.Should().BeFalse();

// Collection assertions
result.Errors.Should().BeEmpty();
result.Errors.Should().HaveCount(2);
result.Errors.Should().Contain(e => e.PropertyName == "Title");
result.Errors.Should().NotContain(e => e.PropertyName == "Description");

// String assertions
error.ErrorMessage.Should().Contain("required");
error.ErrorMessage.Should().StartWith("Title");
error.ErrorMessage.Should().NotBeNullOrEmpty();

// Object assertions
result.Should().NotBeNull();
result.Should().Be(expected); // Reference equality
result.Should().BeEquivalentTo(expected); // Deep comparison

// Numeric assertions
count.Should().Be(3);
count.Should().BeGreaterThan(0);
count.Should().BeLessThanOrEqualTo(10);
```

## Testing Validation Rules

### Required Field
```csharp
[Fact]
public void Validator_EmptyRequiredField_ReturnsValidationError()
{
    // Arrange
    var validator = new CreateIssueValidator();
    var command = new CreateIssueCommand { Title = "" };

    // Act
    var result = validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.PropertyName == "Title" && e.ErrorMessage.Contains("required"));
}
```

### Length Constraints
```csharp
[Fact]
public void Validator_TitleTooShort_ReturnsValidationError()
{
    // Arrange
    var command = new CreateIssueCommand { Title = "AB", Description = "Valid" };

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors[0].PropertyName.Should().Be("Title");
    result.Errors[0].ErrorMessage.Should().Contain("at least 3 characters");
}

[Fact]
public void Validator_TitleTooLong_ReturnsValidationError()
{
    // Arrange
    var command = new CreateIssueCommand
    {
        Title = new string('A', 201), // Exceeds max of 200
        Description = "Valid"
    };

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors[0].ErrorMessage.Should().Contain("cannot exceed 200 characters");
}
```

### Boundary Testing
Always test exact boundaries:

```csharp
[Fact]
public void Validator_TitleExactly3Characters_IsValid()
{
    // Arrange
    var command = new CreateIssueCommand { Title = "Bug", Description = "Valid" };

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeTrue();
}

[Fact]
public void Validator_TitleExactly200Characters_IsValid()
{
    // Arrange
    var command = new CreateIssueCommand
    {
        Title = new string('A', 200), // Exact max
        Description = "Valid"
    };

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeTrue();
}
```

## Mocking with NSubstitute (Future)

When testing services with dependencies, use NSubstitute:

```csharp
[Fact]
public async Task Service_CallsDependency_ReturnsResult()
{
    // Arrange
    var mockRepo = Substitute.For<IIssueRepository>();
    mockRepo.GetByIdAsync("123").Returns(new Issue { Id = "123", Title = "Test" });

    var service = new IssueService(mockRepo);

    // Act
    var result = await service.GetIssueAsync("123");

    // Assert
    result.Should().NotBeNull();
    result.Title.Should().Be("Test");
    
    // Verify method was called
    await mockRepo.Received(1).GetByIdAsync("123");
}
```

## Best Practices

### ✅ Do
- **Test one thing per test** — Focused tests are easier to debug
- **Use descriptive names** — `Validator_EmptyTitle_ReturnsError` not `Test1`
- **Test boundaries** — Min, max, exact values
- **Keep tests fast** — No I/O, <100ms per test
- **Use FluentAssertions** — Readable, detailed failure messages
- **Test both success and failure cases**

### ❌ Don't
- **Test multiple scenarios in one test** — Split into separate tests
- **Use magic numbers** — Use constants or variables with clear names
- **Test implementation details** — Focus on observable behavior
- **Share state between tests** — Each test should be independent
- **Skip boundary tests** — Off-by-one errors are common

## Common Mistakes

### ❌ Testing Too Many Things
```csharp
// Bad — Tests multiple scenarios
[Fact]
public void Validator_MultipleScenarios_Works()
{
    var v = new MyValidator();
    v.Validate(new MyCommand { Title = "" }).IsValid.Should().BeFalse();
    v.Validate(new MyCommand { Title = "AB" }).IsValid.Should().BeFalse();
    v.Validate(new MyCommand { Title = "ABC" }).IsValid.Should().BeTrue();
}
```

### ✅ Split Into Focused Tests
```csharp
// Good — One scenario per test
[Fact]
public void Validator_EmptyTitle_IsInvalid() { /* ... */ }

[Fact]
public void Validator_TitleTooShort_IsInvalid() { /* ... */ }

[Fact]
public void Validator_ValidTitle_IsValid() { /* ... */ }
```

### ❌ Weak Assertions
```csharp
// Bad — Not specific enough
result.Errors.Should().NotBeEmpty();
```

### ✅ Specific Assertions
```csharp
// Good — Tests exact error
result.Errors.Should().HaveCount(1);
result.Errors[0].PropertyName.Should().Be("Title");
result.Errors[0].ErrorMessage.Should().Contain("required");
```

## Test Patterns

### Instance vs. Static Validator
```csharp
// Option 1: Instance per test
[Fact]
public void Test1()
{
    var validator = new MyValidator();
    // ...
}

// Option 2: Shared readonly field
public class MyValidatorTests
{
    private readonly MyValidator _validator = new();

    [Fact]
    public void Test1() { /* use _validator */ }
}
```

### Theory (Data-Driven Tests)
Test multiple inputs with `[Theory]`:

```csharp
[Theory]
[InlineData("")]
[InlineData(" ")]
[InlineData(null)]
public void Validator_EmptyTitle_IsInvalid(string title)
{
    // Arrange
    var command = new CreateIssueCommand { Title = title };

    // Act
    var result = _validator.Validate(command);

    // Assert
    result.IsValid.Should().BeFalse();
}
```

## Running Unit Tests

```bash
# Run all unit tests
dotnet test tests/Unit

# Run specific test
dotnet test --filter "FullyQualifiedName~CreateIssueValidatorTests"

# Run in watch mode
dotnet watch test --project tests/Unit

# Run with coverage
dotnet test tests/Unit --collect:"XPlat Code Coverage"
```

## Debugging Failed Tests

1. **Read the assertion message** — FluentAssertions provides detailed diffs
2. **Set a breakpoint** — In the test method or code under test
3. **Run in Debug mode** — Use Visual Studio's Test Explorer or `dotnet test --filter`
4. **Inspect variables** — Step through the Arrange-Act-Assert phases
5. **Isolate the test** — Run just the failing test, not the entire suite

## See Also

- [Testing Strategy](../TESTING.md) — Overall test philosophy and pyramid
- [Integration Testing Guide](INTEGRATION-TESTS.md) — Testing handlers with MongoDB
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [xUnit Documentation](https://xunit.net/)

---

**Real examples in the codebase:**
- [`tests/Unit/Validators/CreateIssueValidatorTests.cs`](../../tests/Unit/Validators/CreateIssueValidatorTests.cs)
- [`tests/Unit/Validators/UpdateIssueStatusValidatorTests.cs`](../../tests/Unit/Validators/UpdateIssueStatusValidatorTests.cs)
- [`tests/Unit/Domain/IssueTests.cs`](../../tests/Unit/Domain/IssueTests.cs)
