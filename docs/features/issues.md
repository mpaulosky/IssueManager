# Issues API & UI

## Overview

The Issues API provides endpoints to create, read, update, and list issues. The UI surfaces these capabilities through the IssuesPage component with integrated filtering and search.

## GET /api/v1/issues

Retrieves a paginated list of issues with optional filtering and search.

### Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `page` | integer | No | Page number (0-indexed, default: 0) |
| `pageSize` | integer | No | Results per page (default: 10, max: 100) |
| `searchTerm` | string | No | Searches issue titles and descriptions (regex-based, case-insensitive) |
| `authorName` | string | No | Filter by issue author name (exact match, case-insensitive) |
| `statusName` | string | No | Filter by status name (e.g., "Open", "In Progress", "Closed") |
| `categoryName` | string | No | Filter by category name (e.g., "Bug", "Feature Request") |

### Example Requests

**List first 10 issues:**
```
GET /api/v1/issues?page=0&pageSize=10
```

**Search for issues with "login" in title/description:**
```
GET /api/v1/issues?searchTerm=login&page=0&pageSize=10
```

**Filter by author and status:**
```
GET /api/v1/issues?authorName=john&statusName=Open&page=0&pageSize=10
```

**Combine search and filters:**
```
GET /api/v1/issues?searchTerm=auth&categoryName=Bug&statusName=Open&page=0&pageSize=20
```

### Response

Returns an `IssuesPage` object:
```json
{
  "page": 0,
  "pageSize": 10,
  "totalCount": 42,
  "items": [
    {
      "id": "507f1f77bcf86cd799439011",
      "title": "Login page is broken",
      "description": "...",
      "authorName": "john",
      "statusName": "Open",
      "categoryName": "Bug",
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-20T14:22:00Z"
    }
  ]
}
```

## IssuesPage.razor

The IssuesPage component provides a user-friendly interface for browsing and filtering issues.

### Features

- **Pagination:** Navigate through results using page controls
- **Search:** Full-text search across issue titles and descriptions
- **Filter by Author:** Select issues created by a specific author
- **Filter by Status:** Select issues with a specific status
- **Filter by Category:** Select issues with a specific category

### Implementation Notes

All filter selections are wired directly to the API call, ensuring real-time filtering without page reload. Filters can be combined for precise issue lookup.
