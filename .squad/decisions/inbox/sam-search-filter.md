# Decision: Search/Filter Pattern for MongoDB Repositories

**Date:** 2026-02-27  
**Agent:** Sam (Backend Developer)  
**Status:** Implemented

## Context
Extended the Issues list endpoint to support filtering by search term (title/description) and author name. This establishes a pattern for adding filters to paginated MongoDB repository queries.

## Decision
Use MongoDB's `Builders<T>.Filter` API with the following pattern:

1. **Base filters**: Start with required filters (e.g., `Archived == false`)
2. **Optional filters**: Add conditional filters based on non-null/non-empty parameters
3. **Regex matching**: Use `BsonRegularExpression` with `"i"` flag for case-insensitive searches
4. **Combining filters**: Use `Filter.And()` to combine all filters into a single filter definition

## Implementation Pattern

```csharp
var filterBuilder = Builders<Issue>.Filter;
var filters = new List<FilterDefinition<Issue>>
{
    filterBuilder.Eq(x => x.Archived, false)  // Base filter
};

if (!string.IsNullOrWhiteSpace(searchTerm))
{
    var searchFilter = filterBuilder.Or(
        filterBuilder.Regex(x => x.Title, new BsonRegularExpression(searchTerm, "i")),
        filterBuilder.Regex(x => x.Description, new BsonRegularExpression(searchTerm, "i"))
    );
    filters.Add(searchFilter);
}

if (!string.IsNullOrWhiteSpace(authorName))
{
    filters.Add(filterBuilder.Regex(x => x.Author.Name, new BsonRegularExpression(authorName, "i")));
}

var filter = filterBuilder.And(filters);
```

## Interface Changes
When extending repository methods with optional filters:
- Add optional parameters with default `null` values
- Document parameters in XML comments
- Update interface signature first (it's the contract)
- Update all implementations to match
- Update all test mocks to include new parameters (use `null` for existing tests)

## API Layer Flow
1. **Endpoint**: Accept query parameters (`string? searchTerm`, `string? authorName`)
2. **Query object**: Add properties to query record
3. **Validator**: Add validation rules (max length, conditional)
4. **Handler**: Pass query properties to repository
5. **Repository**: Apply MongoDB filters conditionally
6. **API Client**: Build query string, escape parameters with `Uri.EscapeDataString()`

## Why This Matters
- Establishes consistent pattern for future filter additions
- Maintains interface-first approach (contract defines behavior)
- Supports MongoDB's flexible filter composition
- Case-insensitive searches improve UX
- Optional parameters keep API backward-compatible
