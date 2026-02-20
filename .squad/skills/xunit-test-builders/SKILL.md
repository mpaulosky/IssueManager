# xUnit Test Builders — Fluent Test Data Pattern

**Created By:** Gimli (Tester)  
**Date:** 2026-02-20  
**Purpose:** Reusable fluent builder pattern for creating test data with sensible defaults

---

## Overview

Test builders provide a fluent API for creating complex test data objects with sensible defaults, reducing boilerplate and improving test readability.

**Benefits:**
- Sensible defaults (no boilerplate in tests)
- Fluent API (readable, discoverable)
- Consistent test data across suite
- Easy to customize for specific test scenarios
- Self-documenting (builder methods reveal domain model)

---

## Pattern: Fluent Test Data Builder

See: `tests/Unit/Builders/IssueBuilder.cs` for full implementation example.

**Usage:**
```csharp
// Default issue
var issue = IssueBuilder.Default().Build();

// Customized issue
var issue = IssueBuilder.Default()
    .WithTitle("Custom Title")
    .WithDescription("Custom Description")
    .AsClosed()
    .Build();

// Predefined scenario
var closedIssue = IssueBuilder.Closed().Build();
```

---

## Design Guidelines

1. **Sensible Defaults:** Every field should have a valid default value
2. **Fluent Methods:** Return `this` for chaining
3. **Static Factories:** Provide common scenarios (Default, Archived, etc.)
4. **Unique IDs:** Use `Guid.NewGuid()` for automatic uniqueness

---

**Location:** `tests/Unit/Builders/IssueBuilder.cs`  
**Maintained By:** Gimli (Tester)  
**Version:** 1.0
