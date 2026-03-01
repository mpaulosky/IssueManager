# Decision: Forward CancellationToken in ListCategoriesHandler

**Date:** 2026-03-01
**Author:** Sam (Backend Developer)
**Triggered by:** Gimli code review flag

## Problem

`ListCategoriesHandler.Handle()` accepted a `CancellationToken` parameter but silently discarded it — calling `_repository.GetAllAsync()` with no token. This meant cancellation signals (e.g., HTTP request aborted) were ignored at the database layer, risking resource waste and unresponsive cancellation behavior under load.

## Fix Applied

**File:** `src/Api/Handlers/Categories/ListCategoriesHandler.cs` (line 38)

```csharp
// Before
var result = await _repository.GetAllAsync();

// After
var result = await _repository.GetAllAsync(cancellationToken);
```

`ICategoryRepository.GetAllAsync(CancellationToken)` already had the correct signature — the fix was purely a call-site omission.

## Scope Check

- `ListStatusesHandler` was also reviewed and was **already correct** — it forwards `cancellationToken` to `_repository.GetAllAsync(cancellationToken)`.
- No other handlers required changes for this specific bug.

## Outcome

- Build passes with no errors.
- CancellationToken is now correctly propagated to the MongoDB async operation, enabling proper cooperative cancellation.
