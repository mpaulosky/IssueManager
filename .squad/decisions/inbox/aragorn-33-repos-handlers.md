# Decision: Status, Category, Comment Repositories and Handlers (Issue #33)

**Date:** 2026  
**Author:** Aragorn (Lead Developer)  
**Branch:** `squad/33-status-category-comment-handlers`  
**PR:** https://github.com/mpaulosky/IssueManager/pull/45

## Context

Issue #33 required adding concrete MongoDB repository implementations and full CRUD handler stacks for the Status, Category, and Comment models. Interface definitions already existed in `src/Api/Data/` but had no implementations.

## Decisions

### 1. Repository Pattern — Result<T> not DTO-based

The existing `IStatusRepository`, `ICategoryRepository`, and `ICommentRepository` interfaces use the `Result<T>` pattern from `Shared.Abstractions`. The `IssueRepository` uses a DTO-based pattern. We matched the interfaces as specified, using `Result<T>` in the new repositories.

### 2. Handler Error Propagation

Handlers convert `Result.Failure` into typed exceptions (`NotFoundException`, `ValidationException`) to integrate with the HTTP layer — matching the Issues handler pattern.

### 3. CommentRepository.GetByUserAsync

Filtered by `x.Author.Id` (UserDto field is `Id`, not `UserId`). Confirmed by inspecting `UserDto.cs`.

### 4. Category.DateCreated is init-only

`Category.DateCreated` is declared as `init`, so it cannot be mutated after construction. Handlers set it at creation time only.

### 5. Endpoints

- `GET/POST /api/v1/statuses` — list / create
- `GET/PATCH/DELETE /api/v1/statuses/{id}` — get / update / archive
- Same pattern for `/api/v1/categories` and `/api/v1/comments`

## Result

- 30 files changed, 1810 insertions
- Build: 0 errors, 0 warnings
- Tests: 84/84 passing
