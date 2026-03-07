# Decision: Split Integration Collection into 4 Parallel Domain Collections

**Date:** 2026-03-07
**Author:** Gimli (Tester)
**Status:** Adopted

## Context

The `Api.Tests.Integration` suite was taking 5–10 minutes to run because:
1. Every one of the 23 test classes created its own `MongoDbContainer` (~2s startup each = ~46s wasted)
2. The single `[CollectionDefinition("Integration", DisableParallelization = true)]` forced all tests to run sequentially
3. `parallelizeTestCollections: false` in `xunit.runner.json` disabled collection-level parallelism

## Decision

Replace the single `"Integration"` collection with four domain-specific collections, each backed by a shared `ICollectionFixture<MongoDbFixture>`:

| Collection | Classes | Container |
|---|---|---|
| `CategoryIntegration` | 5 (4 handlers + CategoryRepository) | 1 shared |
| `IssueIntegration` | 9 (7 handlers + IssueRepositorySearch + IssueRepository) | 1 shared |
| `CommentIntegration` | 5 (5 comment handlers) | 1 shared |
| `StatusIntegration` | 4 (4 status handlers) | 1 shared |

Enable `parallelizeTestCollections: true` in `xunit.runner.json`. The 4 collections now run in parallel.

## Isolation Strategy

Each test class constructor uses a Guid-based database name:
```csharp
_repository = new XxxRepository(fixture.ConnectionString, $"T{Guid.NewGuid():N}");
```
Since xUnit v3 creates a new class instance per test method, every test method gets a fresh MongoDB database. This maintains the same isolation level as the old per-class container approach.

## Impact

- Containers: 23 → 4 (parallel)
- Expected runtime: 5–10 min → ~2–3 min
- Test isolation: maintained (unique DB per test method via Guid)

## Rule Change

**Old Critical Rule 2:** "`[Collection("Integration")]` REQUIRED on all integration test classes"

**New Critical Rule 2:** Use domain-specific collections (`CategoryIntegration`, `IssueIntegration`, `CommentIntegration`, `StatusIntegration`). Do NOT use the old single `Integration` collection.
