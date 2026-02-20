# 2026-02-20: Base Model Abstraction Decision

**By:** Gandalf  
**Date:** 2026-02-20  
**Requested by:** mpaulosky

## The Question

Should we create an abstract base class to handle common properties (Id, CreatedOn, ModifiedOn, Archived, ArchivedBy) across domain models, or handle these fields independently in each model?

---

## Analysis

### Current State

Our models are **records** using **primary constructor** patterns:
- `Issue`: Has `Id`, `CreatedAt`, `UpdatedAt`, `IsArchived`
- `Comment`: Has `Id`, `CreatedAt`, no modification tracking or archive
- `User`, `Category`, `Status`: Have `Id` only, minimal audit trail

**Observation:** Fields are inconsistent by design—each model owns what it needs. There's duplication in some places (e.g., `Id`), but not yet a pervasive pattern that demands extraction.

### Vertical Slice Implications

Our architecture is **Vertical Slice Architecture** with CQRS. Each feature slice (e.g., "Create Issue", "Update Issue Status") owns its domain models end-to-end. A shared base class creates implicit **cross-slice coupling**:

- **Pro:** Single source of truth for audit fields.
- **Con:** All slices depend on a shared base. Changes to auditing logic affect the entire codebase. Future slices must inherit the base even if they don't need all fields.

In vertical slices, we prefer **feature autonomy** over DRY. A little duplication is acceptable if it keeps slices independent.

### MongoDB.EntityFramework + EF Core Inheritance Consideration

MongoDB.EntityFramework supports **table-per-type (TPT) inheritance**, meaning:
- Base class can be mapped to its own collection
- Derived types create their own collections
- Or: **single-collection pattern** where all types map to one collection with discriminators

**Practical impact:** Inheritance in EF Core + MongoDB works, but:
1. Adds complexity to queries (must handle polymorphic queries correctly)
2. MongoDB.EntityFramework is young—inheritance edge cases aren't as battle-tested as SQL EF Core
3. Records + records inheritance = awkward primary constructor chaining

### Code Reuse vs. Composition

Current model structure uses **records**, which are immutable and validation-heavy (constructor guards). A base class would:
- Share validation logic for `Id`, timestamp defaults
- Increase inheritance depth
- Make records harder to extend (primary constructor must call base)

**Pattern observation:** We validate once at construction. Sharing that validation across models is nice, but the duplication is small—guards are ~3 lines per field.

### Testing Implications

If we add a base class:
- Test fixtures must construct base properties correctly
- Mock models inherit from base, adding indirection
- Builders (if we use them) become more complex

Current flat structure = simpler test fixtures, no inheritance to navigate.

---

## Trade-offs Summary

| Aspect | Base Class | No Base Class |
|--------|-----------|---------------|
| **Code reuse** | ✅ Centralized audit logic | ❌ Duplication if many models need it |
| **Vertical slice coupling** | ❌ Cross-slice dependency | ✅ Feature autonomy maintained |
| **Model clarity** | ⚠️ Inheritance hierarchy to understand | ✅ Each model self-contained |
| **MongoDB.EF complexity** | ⚠️ Inheritance mapping adds complexity | ✅ Simpler collection design |
| **Testing** | ⚠️ Fixture inheritance chains | ✅ Direct construction |
| **Flexibility** | ⚠️ All models must conform | ✅ Fields per model, not per base |

---

## Recommendation

**DO NOT create a base class now.**

### Rationale

1. **Premature Abstraction:** We have 4 models. Only 2 (`Issue`, `Comment`) show consistent audit needs. `User`, `Category`, `Status` need only `Id`. This is not yet a pattern; it's coincidence.

2. **Vertical Slice Principle:** Base classes create cross-slice coupling. If "Manage Issues" and "Manage Comments" are separate slices, they should not depend on shared base classes. If audit logic changes, only the affected slices should recompile.

3. **MongoDB.EntityFramework Maturity:** The library is still evolving. Inheritance with records + MongoDB mapping is less tested than flat record structures. We should avoid this complexity until proven necessary.

4. **Future Flexibility:** Once we reach 6-10 models with clear audit patterns, we'll have real data. At that point, introducing an interface (`IAuditable`) or extension methods is safer than inheritance.

### Conditional Exception

If **all** future domain models require identical audit fields (`Id`, `CreatedOn`, `ModifiedOn`, `Archived`, `ArchivedBy`), **reconsider at the next architecture review** (after 5+ models exist with the full pattern). Then, a small **interface + default implementation** (C# 8 default interface members) is preferable to inheritance.

### What to Do Instead

**Option A: Interface + Extension Methods (Future)**
```csharp
public interface IAuditable
{
    string Id { get; }
    DateTime CreatedOn { get; }
    DateTime? ModifiedOn { get; }
    bool Archived { get; }
    string? ArchivedBy { get; }
}

public static class AuditableExtensions
{
    public static bool IsActive(this IAuditable entity) => !entity.Archived;
}
```
- Loose coupling, no inheritance chain
- Models explicitly declare audit intent
- Easier to test and mock

**Option B: Composition (Current)**
- Each model declares its own fields
- Validation guards duplicated (acceptable for now)
- No cross-slice coupling
- Clear ownership

**Recommended:** Stay with **Option B** (composition) until models reach 8+, then evaluate **Option A** (interface) if patterns emerge.

---

## Decision Record

- **When to revisit:** After 5+ domain models are in place and audit patterns are clear
- **Team discussion:** Should occur before any base class implementation
- **Responsible agent:** Aragorn (Backend) to monitor model growth and flag when reconsideration is warranted
- **Owner:** Gandalf

