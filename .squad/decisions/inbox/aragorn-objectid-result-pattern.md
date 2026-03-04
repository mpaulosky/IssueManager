# Decision: ObjectId/Result<T> Validation & Propagation Pattern

**Date:** 2026-03-03  
**Author:** Aragorn (Lead Developer)  
**Status:** Active (sprint #80–#90)

## Problem

All four API domains (Issues, Categories, Statuses, Comments) have three critical architectural issues:

1. **ObjectId vs String inconsistency**: Commands/Queries mix `string` and `ObjectId` Id fields; `UpdateIssueCommand.Id` is typed as `ObjectId?` but initialized with `string.Empty` (type mismatch bug)
2. **Runtime ObjectId parsing in handlers**: `ObjectId.TryParse()` called in handler bodies instead of at validation boundary
3. **Missing Result<T> propagation**: Handlers throw exceptions or return DTOs directly; don't return `Result<T>` to endpoints for HTTP status code mapping

## Decision

### 1. ObjectId Parsing Moves to Validation Layer (FluentValidation)

**Pattern:**
- HTTP endpoint accepts **string** ID from client
- FluentValidation validator parses `string` → `ObjectId` **before** handler runs
- Handler receives strongly-typed `ObjectId`, never null, never invalid
- **No `ObjectId.TryParse()` calls in handler bodies**

**Benefit:** Validation failures → 400 Bad Request at boundary; handlers assume valid ObjectId.

**Files affected:**
- All Delete/Update Command classes: `Id: ObjectId` (not `string`)
- All corresponding FluentValidation validators: implement string→ObjectId parsing
- All Get Query classes with string IDs: validators parse to ObjectId

### 2. All Handlers Return Task<Result<T>>

**Pattern:**
- Handler signature: `Task<Result<TDto>>` (not `Task<TDto?>`, `Task<bool>`, `Task<IssueDto>`)
- Result<T> wraps success/failure: `Result<IssueDto>.Success(dto)` or `Result<IssueDto>.Failure(error)`
- Repositories already return `Result<T>` internally; handlers unwrap and re-wrap with domain logic results

**Benefit:** Consistent error handling across all handlers; endpoints have clear success/failure mapping.

**Files affected:**
- All handler classes: update return type to `Task<Result<T>>`
- All endpoint classes: map Result → HTTP status (200/201 success, 404/409/500 failure)

### 3. Endpoints Map Result<T> to HTTP Status Codes

**Pattern:**
```csharp
var result = await mediator.Send(command);
return result.Match(
    onSuccess: dto => Results.Created($"/resource/{dto.Id}", dto),
    onFailure: failure => failure.Type switch {
        FailureType.NotFound => Results.NotFound(),
        FailureType.Conflict => Results.Conflict(),
        _ => Results.BadRequest(failure.Error)
    }
);
```

**Benefit:** Clear HTTP semantics; clients get proper status codes instead of 200 with error bodies.

## Scope

**Domains affected (all four):**
- Issues: handlers, tests, endpoints
- Categories: handlers, tests, endpoints
- Statuses: handlers, tests, endpoints
- Comments: handlers, tests, endpoints

**Test impact:** ~53 test files need updates for Result<T> assertions and ObjectId-compatible ID setup.

## Implementation Order

1. **Foundation (#80):** Fix all Shared command/query types and validators
2. **Parallel domain streams (#81-#89):**
   - Sam: handler refactors (Issues, Categories, Statuses, Comments)
   - Gimli: test updates for each domain
3. **Infrastructure (#89):** Aspire startup fixes
4. **Integration gate (#90):** Full build + all tests green

## References

- Sprint issues: #80–#90
- Related pattern: Vertical Slice Architecture (each domain owns handlers + endpoints + tests)
- Validation library: FluentValidation with custom string→ObjectId rule
- Result type: `src/Shared/Abstractions/Result.cs` (already exists)

## Sign-off

✓ Aragorn (Lead Developer) — approved  
✓ Matthew Paulosky (Project Owner) — validation pending
