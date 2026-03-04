### ObjectId parsing belongs at the endpoint boundary, not in handlers
**By:** Sam (Backend Developer)
**Date:** 2026-03-04
**PR:** #91 (issue #80)

**What:**
The established pattern for ObjectId handling across all 4 API domains (Issues, Categories, Statuses, Comments) is:

1. **Endpoints** accept `string id` from the URL path parameter and call `ObjectId.TryParse(id, out var objectId)`. If parsing fails, return `Results.BadRequest("Invalid ID format")` immediately.
2. **Commands/Queries** hold strongly-typed `ObjectId Id` (never `string`, never `ObjectId?`). Default initializer `= string.Empty` on an ObjectId property is a type-mismatch bug — remove it; structs auto-initialize to `default` (ObjectId.Empty).
3. **Handlers** receive the ObjectId via the command/query and pass it directly to repository methods. No `ObjectId.TryParse()` inside handler bodies.
4. **Web/Blazor pages** that construct commands from URL route parameters (string) must call `ObjectId.Parse(routeParam)` when setting the Id property.

**Why:**
- Type safety: strongly-typed ObjectId eliminates the need for repeated string→ObjectId parsing deep in the stack
- Fail-fast: invalid IDs produce 400 Bad Request at the HTTP boundary, before any handler logic runs
- Cleaner handlers: handlers focus on business logic, not input parsing

**Affected files pattern:**
- `src/Shared/Validators/Delete*Command.cs`, `Update*Command.cs` — Id property type
- `src/Api/Handlers/*/Get*Handler.cs`, `Delete*Handler.cs`, `Update*Handler.cs` — remove TryParse
- `src/Api/Handlers/*Endpoints.cs` — add TryParse guard before command creation
- `src/Web/_Imports.razor` — add `@using MongoDB.Bson` for ObjectId access in Blazor
