# Decision: Handlers Generate ObjectIds, Repositories Validate

**Date:** 2026-03-10
**Author:** Gimli (Tester)
**Status:** Implemented

## Context

The Create handlers (CreateIssueHandler, CreateCommentHandler, CreateCategoryHandler, CreateStatusHandler) were passing `ObjectId.Empty` to repository CreateAsync methods, expecting the repository or MongoDB to generate the ID.

However, the repositories now validate that IDs are not `ObjectId.Empty` before performing create operations, causing all Create handler integration tests to fail with "ID cannot be empty" errors.

## Decision

**Handlers are responsible for generating new ObjectIds**, not repositories.

- Create handlers call `ObjectId.GenerateNewId()` when constructing DTOs/models
- Repositories validate that IDs are not empty before database operations
- This separation ensures explicit ID ownership at the application layer

## Rationale

1. **Explicit is better than implicit** — The handler knows it's creating a new entity and should assign the ID
2. **Repository validation prevents bugs** — Empty ID validation catches accidental omissions
3. **Testability** — Unit tests can verify the handler generates a valid ID before calling the repository
4. **MongoDB compatibility** — While MongoDB can auto-generate `_id`, our BSON mapping uses ObjectId as the ID type, so pre-generation is cleaner

## Affected Files

- `src/Api/Handlers/Comments/CreateCommentHandler.cs`
- `src/Api/Handlers/Statuses/CreateStatusHandler.cs`
- `src/Api/Handlers/Categories/CreateCategoryHandler.cs`
- `src/Api/Handlers/Issues/CreateIssueHandler.cs`

## For Future Development

When creating new Create handlers, always use `ObjectId.GenerateNewId()` when constructing the DTO/model. Do NOT rely on the repository or database to generate the ID.
