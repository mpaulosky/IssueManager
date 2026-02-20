# Testing Documentation Decision — Gimli (2026-02-19)

## Decision: Comprehensive Test Documentation Structure

**By:** Gimli (Quality Assurance)  
**Date:** 2026-02-19  
**Status:** Implemented

## What
Created 8 testing documentation files covering all test types, frameworks, patterns, and best practices:
1. **TESTING.md** — Main strategy doc (test pyramid, coverage goals, quality gates)
2. **guides/UNIT-TESTS.md** — xUnit, FluentValidation, FluentAssertions
3. **guides/BUNIT-BLAZOR-TESTS.md** — bUnit, component lifecycle, parameters, callbacks
4. **guides/ARCHITECTURE-TESTS.md** — NetArchTest, layer boundaries, design rules
5. **guides/INTEGRATION-TESTS.md** — TestContainers, MongoDB, vertical slices
6. **guides/E2E-PLAYWRIGHT-TESTS.md** — Playwright, browser automation, workflows
7. **guides/TEST-DATA.md** — Builders, factories, fixtures, isolation
8. **CONTRIBUTING.md** — Updated with testing section and quality checklist

## Why
- **Onboarding:** New team members can learn testing practices quickly
- **Consistency:** Standardized patterns across all test types
- **Quality gates:** Clear expectations for test coverage and quality
- **Knowledge preservation:** Documents why we use specific frameworks and patterns
- **Self-service:** Developers can find answers without asking

## Key Patterns Established

### Test Pyramid
```
     /\     E2E Tests (~15 tests)
    /  \    ↓ Slow, high coverage, critical workflows
   /____\   Integration Tests (~17 tests)
  /      \  ↓ Vertical slices, MongoDB, handlers + validators
 /________\ Unit Tests (~30 tests) + Architecture Tests (~10 tests)
            ↓ Fast, focused, one thing per test
```

### Coverage Goals
- **80%+ for handlers and validators** (business logic)
- **60%+ for Blazor components** (UI interactions)
- **100% for architecture rules** (design constraints)
- **Critical paths covered** by integration and E2E tests

### Framework Choices
- **Unit:** xUnit, FluentValidation, FluentAssertions (fast, focused, readable)
- **Architecture:** NetArchTest.Rules (enforce layer boundaries, naming conventions)
- **Integration:** TestContainers (real MongoDB, isolated containers)
- **Blazor:** bUnit (component rendering, lifecycle, parameters, callbacks)
- **E2E:** Playwright (browser automation, critical workflows)

### Documentation Structure
Each guide follows a consistent template:
1. **Overview** — What, when, why
2. **Setup** — How to create a test file
3. **Examples** — Real code from the codebase
4. **Best Practices** — ✅ Do / ❌ Don't
5. **Common Mistakes** — Anti-patterns with corrections
6. **Debugging** — How to diagnose failures
7. **Running Tests** — Commands and options
8. **See Also** — Cross-references

### Test Naming Convention
```
[MethodUnderTest]_[Scenario]_[ExpectedOutcome]
```

Examples:
- `Handle_ValidCommand_StoresIssueInDatabase`
- `IssueForm_ShowsUpdateButtonText_WhenIsEditModeIsTrue`
- `CreateIssueValidator_EmptyTitle_ReturnsValidationError`

### Test Structure
All tests follow **Arrange-Act-Assert** (AAA) or **Given-When-Then** patterns:
```csharp
[Fact]
public void MethodUnderTest_Scenario_ExpectedOutcome()
{
    // Arrange — Set up test data and dependencies
    var input = /* ... */;

    // Act — Execute the code under test
    var result = /* ... */;

    // Assert — Verify the outcome
    result.Should()./* assertion */;
}
```

## Impact

### Developers
- Self-service documentation for writing tests
- Clear examples and patterns to follow
- Reduced questions and blockers

### Code Quality
- Standardized test patterns across the codebase
- Consistent coverage expectations
- Architecture rules enforced automatically

### Maintenance
- Tests are maintainable and readable
- New team members can contribute tests confidently
- Documentation lives with the code

## Next Steps

1. **Review docs for typos/clarity** (Gimli)
2. **Get feedback from team** (Gandalf, Aragorn, Arwen)
3. **Link from README.md** (optional)
4. **Update as patterns evolve**

## References
- All test guides: `docs/TESTING.md` and `docs/guides/`
- Real test examples: `tests/Unit/`, `tests/Integration/`, `tests/BlazorTests/`, `tests/Architecture/`
- Updated contributing guide: `docs/CONTRIBUTING.md`
