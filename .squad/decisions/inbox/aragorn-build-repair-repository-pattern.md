# Decision: Repository Pattern — Interface as Contract

**Date**: 2026-02-26  
**Status**: Implemented  
**Decision Maker**: Aragorn (Lead Developer)

## Context

During build repair of `src\Api\Api.csproj`, discovered 14 compilation errors caused by mismatched method signatures between repository interfaces and their callers (handlers). The repository implementations had already been updated to match the new interface signatures, but handler code was still calling the old method names.

### The Problem

Handlers were calling:
- `GetAsync(objectId)` → Interface defined `GetByIdAsync(objectId)`
- `CreateAsync(model)` → Interface expected `CreateAsync(dto)`
- `UpdateAsync(objectId, dto)` → Interface expected `UpdateAsync(dto)`
- `ArchiveAsync(dto)` → Interface expected `ArchiveAsync(objectId)`

## Decision

**The interface defines the contract. Always update implementations and callers to match the interface, never change the interface to match old caller code.**

### Rationale

1. **Interfaces are contracts**: They define the public API that consumers depend on
2. **Backwards compatibility**: Changing an interface breaks all implementations and callers
3. **Single source of truth**: The interface should be the authoritative definition
4. **Testability**: Mock implementations must match the interface exactly

### Implementation Pattern

When fixing repository/handler mismatches:

1. ✅ **Correct approach**: Update handler code to match interface
   ```csharp
   // Interface defines
   Task<Result<CategoryDto>> GetByIdAsync(ObjectId id, CancellationToken ct);
   
   // Handler calls
   var result = await _repository.GetByIdAsync(objectId, cancellationToken);
   ```

2. ❌ **Incorrect approach**: Change interface to match old handlers
   ```csharp
   // Don't change interface back to GetAsync just because handlers call it
   ```

### DTO vs Model Handling

Create handlers must construct DTOs, not Models:

```csharp
// ✅ Correct
var dto = new CategoryDto(ObjectId.Empty, name, description, DateTime.UtcNow, null, false, null);
var result = await _repository.CreateAsync(dto, cancellationToken);
return result.Value;

// ❌ Incorrect
var model = new Category { CategoryName = name, ... };
var result = await _repository.CreateAsync(model); // Type mismatch!
return model.ToDto();
```

## Consequences

### Positive
- Clear contract definition via interfaces
- Type safety at compile time
- Consistent pattern across all repositories
- Easier to mock for testing

### Negative
- More verbose handler code (must construct full DTOs)
- Nullable warnings when constructing DTOs with null fields

### Neutral
- Test project builders need updating separately (out of scope for this task)

## Related Files

- Repository interfaces: `src/Api/Data/I*Repository.cs`
- Repository implementations: `src/Api/Data/*Repository.cs`
- Handlers: `src/Api/Handlers/{Categories,Statuses,Comments,Issues}/*Handler.cs`
- Mappers: `src/Shared/Mappers/*Mapper.cs`

## Follow-up Actions

- [ ] Update test project builders to match new DTO constructors
- [ ] Consider factory methods on DTOs to reduce constructor verbosity
- [ ] Document DTO construction patterns in CONTRIBUTING.md
