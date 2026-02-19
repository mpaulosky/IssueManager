# Architecture Testing Guide

## Overview

Architecture tests enforce design rules and constraints at compile time. They use reflection to analyze assemblies and verify that your code follows architectural principles (layer dependencies, naming conventions, etc.).

**When to use architecture tests:**
- Enforcing layer boundaries (e.g., Domain must not depend on Infrastructure)
- Verifying naming conventions (e.g., all validators end with "Validator")
- Ensuring dependency rules (e.g., no circular dependencies)
- Enforcing immutability (e.g., domain models are records)
- Preventing unwanted dependencies (e.g., domain doesn't depend on MongoDB)

**Framework used:**
- **NetArchTest.Rules** — Fluent API for architecture rules

## Setup

### Create an Architecture Test File

1. Add test file to `tests/Architecture/`
2. Reference NetArchTest via GlobalUsings:
   ```csharp
   // tests/Architecture/GlobalUsings.cs
   global using Xunit;
   global using FluentAssertions;
   global using NetArchTest.Rules;
   ```

3. Create test class:
   ```csharp
   namespace IssueManager.Tests.Architecture;

   /// <summary>
   /// Architecture tests that enforce project structure and dependencies.
   /// </summary>
   public class ArchitectureTests
   {
       // Tests go here
   }
   ```

## Example: Layer Dependency Rule

**Real example from the codebase:** [`tests/Architecture/ArchitectureTests.cs`](../../tests/Architecture/ArchitectureTests.cs)

```csharp
[Fact]
public void SharedLayer_ShouldNotDependOnHigherLayers()
{
    // Arrange
    var sharedAssembly = typeof(IssueManager.Shared.Domain.Label).Assembly;

    // Act
    var result = Types.InAssembly(sharedAssembly)
        .That()
        .ResideInNamespace("IssueManager.Shared")
        .ShouldNot()
        .HaveDependencyOnAny("IssueManager.Api", "IssueManager.Web")
        .GetResult();

    // Assert
    result.IsSuccessful.Should().BeTrue(
        "Shared layer is the foundation and should have no dependencies on Api or Web layers");
}
```

## NetArchTest Basics

### Structure of a Rule
```csharp
var result = Types.InAssembly(assembly)
    .That()                                  // Filter criteria (optional)
    .ResideInNamespace("MyNamespace")
    .Should() / .ShouldNot()                 // Assertion
    .HaveDependencyOn("DependencyName")
    .GetResult();                            // Execute rule

result.IsSuccessful.Should().BeTrue("Reason for rule");
```

### Common Filters (That)
```csharp
// By namespace
.That().ResideInNamespace("IssueManager.Shared.Domain")

// By name pattern
.That().HaveNameEndingWith("Validator")
.That().HaveNameStartingWith("Create")

// By type
.That().AreClasses()
.That().AreInterfaces()
.That().ArePublic()

// Combine filters
.That()
    .ResideInNamespace("MyNamespace")
    .And()
    .AreClasses()
```

### Common Assertions (Should/ShouldNot)
```csharp
// Dependencies
.Should().HaveDependencyOn("FluentValidation")
.ShouldNot().HaveDependencyOn("MongoDB")
.ShouldNot().HaveDependencyOnAny("Api", "Web")

// Inheritance
.Should().Inherit<BaseClass>()
.Should().ImplementInterface<IMyInterface>()

// Naming
.Should().HaveNameEndingWith("Validator")
.Should().HaveNameStartingWith("Create")

// Attributes
.Should().HaveCustomAttribute<MyAttribute>()
```

## Example Architecture Rules

### 1. Domain Models Should Not Depend on Infrastructure
```csharp
[Fact]
public void DomainModels_ShouldNotDependOnInfrastructure()
{
    // Arrange
    var sharedAssembly = typeof(IssueManager.Shared.Domain.Issue).Assembly;

    // Act
    var result = Types.InAssembly(sharedAssembly)
        .That()
        .ResideInNamespace("IssueManager.Shared.Domain")
        .ShouldNot()
        .HaveDependencyOn("MongoDB")
        .GetResult();

    // Assert
    result.IsSuccessful.Should().BeTrue(
        "Domain models must be persistence-agnostic and not depend on MongoDB or any other infrastructure");
}
```

### 2. Validators Should Follow Naming Convention
```csharp
[Fact]
public void Validators_ShouldFollowNamingConvention()
{
    // Arrange
    var sharedAssembly = typeof(IssueManager.Shared.Validators.CreateIssueValidator).Assembly;

    // Act
    var result = Types.InAssembly(sharedAssembly)
        .That()
        .ResideInNamespace("IssueManager.Shared.Validators")
        .And()
        .AreClasses()
        .Should()
        .HaveNameEndingWith("Validator")
        .Or()
        .HaveNameEndingWith("Command")
        .GetResult();

    // Assert
    result.IsSuccessful.Should().BeTrue(
        "All validator classes should end with 'Validator' or be command DTOs ending with 'Command'");
}
```

### 3. Validators Should Only Depend on FluentValidation
```csharp
[Fact]
public void Validators_ShouldOnlyDependOnFluentValidationAndDomain()
{
    // Arrange
    var sharedAssembly = typeof(IssueManager.Shared.Validators.CreateIssueValidator).Assembly;

    // Act
    var result = Types.InAssembly(sharedAssembly)
        .That()
        .ResideInNamespace("IssueManager.Shared.Validators")
        .And()
        .HaveNameEndingWith("Validator")
        .Should()
        .HaveDependencyOn("FluentValidation")
        .GetResult();

    // Assert
    result.IsSuccessful.Should().BeTrue(
        "Validators must depend on FluentValidation for validation logic");
}
```

### 4. Validators Should Not Depend on Higher Layers
```csharp
[Fact]
public void Validators_ShouldNotDependOnHigherLayers()
{
    // Arrange
    var sharedAssembly = typeof(IssueManager.Shared.Validators.CreateIssueValidator).Assembly;

    // Act
    var result = Types.InAssembly(sharedAssembly)
        .That()
        .ResideInNamespace("IssueManager.Shared.Validators")
        .ShouldNot()
        .HaveDependencyOnAny("IssueManager.Api", "IssueManager.Web")
        .GetResult();

    // Assert
    result.IsSuccessful.Should().BeTrue(
        "Validators are pure logic and should not depend on Api or Web layers");
}
```

### 5. Domain Models Should Be Records (Immutable)
```csharp
[Fact]
public void DomainModels_ShouldBeRecords()
{
    // Arrange
    var sharedAssembly = typeof(IssueManager.Shared.Domain.Issue).Assembly;

    // Act
    var domainTypes = Types.InAssembly(sharedAssembly)
        .That()
        .ResideInNamespace("IssueManager.Shared.Domain")
        .GetTypes()
        .Where(t => !t.IsEnum)
        .ToList();

    // Assert
    foreach (var type in domainTypes)
    {
        // Records are classes with specific compiler-generated members
        var isRecord = type.GetMethod("<Clone>$") is not null;
        isRecord.Should().BeTrue(
            $"Domain model '{type.Name}' should be a record for immutability");
    }
}
```

### 6. Api Layer Should Not Depend on Web Layer
```csharp
[Fact]
public void ApiLayer_ShouldNotDependOnWebLayer()
{
    // Arrange
    var apiAssembly = AppDomain.CurrentDomain.GetAssemblies()
        .FirstOrDefault(a => a.GetName().Name == "Api");

    // Skip test if Api assembly is not loaded
    if (apiAssembly is null)
    {
        return; // Acceptable in isolated test runs
    }

    // Act
    var result = Types.InAssembly(apiAssembly)
        .That()
        .ResideInNamespace("IssueManager.Api")
        .ShouldNot()
        .HaveDependencyOn("IssueManager.Web")
        .GetResult();

    // Assert
    result.IsSuccessful.Should().BeTrue(
        "Api layer should not depend on Web layer to maintain separation of concerns");
}
```

### 7. ServiceDefaults Should Have Minimal Dependencies
```csharp
[Fact]
public void ServiceDefaults_ShouldHaveMinimalDependencies()
{
    // Arrange
    var serviceDefaultsAssembly = typeof(IssueManager.ServiceDefaults.Extensions).Assembly;

    // Act
    var result = Types.InAssembly(serviceDefaultsAssembly)
        .That()
        .ResideInNamespace("IssueManager.ServiceDefaults")
        .ShouldNot()
        .HaveDependencyOnAny("IssueManager.Api", "IssueManager.Web", "IssueManager.Shared")
        .GetResult();

    // Assert
    result.IsSuccessful.Should().BeTrue(
        "ServiceDefaults should only contain cross-cutting infrastructure concerns without business logic dependencies");
}
```

## Why Architecture Tests Matter

### Prevent Accidental Dependencies
Without tests, developers might accidentally:
- Reference Web from Api
- Use MongoDB types in Domain
- Skip naming conventions

### Document Architectural Decisions
Tests serve as executable documentation:
- "Why can't I use MongoDB in Domain?" → See the architecture test
- "What's the naming convention for validators?" → See the test

### Enforce Clean Architecture
- **Domain** — Pure business logic (no infra)
- **Application** — Handlers, validators (depend on domain)
- **Infrastructure** — MongoDB, APIs (depend on application)
- **Presentation** — Web, API (depend on application)

## How to Add New Architecture Rules

### Step 1: Identify the Rule
What constraint do you want to enforce?
- "All handlers should follow CQRS pattern"
- "All repositories should implement IRepository"
- "All DTOs should be records"

### Step 2: Write the Test
```csharp
[Fact]
public void Handlers_ShouldFollowCQRSNamingConvention()
{
    var assembly = typeof(CreateIssueHandler).Assembly;

    var result = Types.InAssembly(assembly)
        .That()
        .ResideInNamespace("IssueManager.Shared.Handlers")
        .Should()
        .HaveNameEndingWith("Handler")
        .GetResult();

    result.IsSuccessful.Should().BeTrue(
        "All handlers should end with 'Handler' to follow CQRS conventions");
}
```

### Step 3: Run the Test
```bash
dotnet test tests/Architecture --filter "FullyQualifiedName~Handlers_ShouldFollowCQRSNamingConvention"
```

### Step 4: Fix Violations or Update Rule
- If the test fails, either fix the code or adjust the rule
- Document why the rule exists in the test assertion message

## Best Practices

### ✅ Do
- **Write descriptive assertion messages** — Explain why the rule exists
- **Test one architectural constraint per test** — Focused, clear failures
- **Use meaningful test names** — `DomainModels_ShouldNotDependOnInfrastructure`
- **Document intent** — XML comments on test methods
- **Run architecture tests in CI** — Catch violations early

### ❌ Don't
- **Test implementation details** — Focus on architectural constraints
- **Over-constrain** — Only enforce rules that add value
- **Ignore failures** — Architecture tests should always pass
- **Test external libraries** — Focus on your own code

## Common Mistakes

### ❌ Vague Assertion Messages
```csharp
result.IsSuccessful.Should().BeTrue();
```

### ✅ Descriptive Assertion Messages
```csharp
result.IsSuccessful.Should().BeTrue(
    "Domain models must be persistence-agnostic and not depend on MongoDB or any other infrastructure");
```

### ❌ Testing Multiple Rules in One Test
```csharp
// Bad — Tests naming AND dependencies
[Fact]
public void Validators_ShouldFollowAllRules()
{
    // Test naming convention
    // Test dependencies
    // Test inheritance
}
```

### ✅ Split Into Focused Tests
```csharp
[Fact]
public void Validators_ShouldFollowNamingConvention() { /* ... */ }

[Fact]
public void Validators_ShouldNotDependOnHigherLayers() { /* ... */ }

[Fact]
public void Validators_ShouldInheritFromAbstractValidator() { /* ... */ }
```

## Debugging Architecture Test Failures

### Read the Assertion Message
NetArchTest provides detailed failure information:
```
Expected result.IsSuccessful to be true because Domain models must be persistence-agnostic, but found 2 violations:
  - IssueManager.Shared.Domain.Issue
  - IssueManager.Shared.Domain.Label
```

### Identify Violating Types
The failure message lists types that violate the rule. Check:
- What dependencies do they have?
- Are they in the wrong namespace?
- Do they follow the naming convention?

### Fix or Adjust the Rule
- **Fix the code** — Remove the violating dependency/pattern
- **Adjust the rule** — If the rule is too strict, update the test

## Running Architecture Tests

```bash
# Run all architecture tests
dotnet test tests/Architecture

# Run specific test
dotnet test --filter "FullyQualifiedName~DomainModels_ShouldNotDependOnInfrastructure"

# Run in watch mode
dotnet watch test --project tests/Architecture
```

## See Also

- [Testing Strategy](../TESTING.md) — Overall test philosophy
- [NetArchTest Documentation](https://github.com/BenMorris/NetArchTest)
- [Clean Architecture Principles](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

---

**Real examples in the codebase:**
- [`tests/Architecture/ArchitectureTests.cs`](../../tests/Architecture/ArchitectureTests.cs)
