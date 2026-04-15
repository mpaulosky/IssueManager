# Categories API & UI

## Overview

Categories allow issues to be classified by type (e.g., "Bug", "Feature Request", "Documentation"). Admins can manage categories, including archiving (soft-deleting) them to prevent new usage while preserving historical data.

## DELETE /api/v1/categories/{id}

Archives (soft-deletes) a category. Only administrators can perform this action.

### Authorization

- **Required Role:** Administrator
- **Response:** `204 No Content` on success or `403 Forbidden` if user lacks permissions

### Behavior

When a category is archived:
- The category document is marked with `Archived = true`
- The `ArchivedBy` field is set to the current user's ID
- The `ArchivedAt` timestamp is recorded
- Archived categories **do not appear** in issue creation forms
- Existing issues with archived categories remain visible but cannot be reassigned to that category
- The category can be restored by a squad member with database access (manual intervention)

### Example Request

```
DELETE /api/v1/categories/507f1f77bcf86cd799439011
Authorization: Bearer {token}
```

### Example Response

```
HTTP/1.1 204 No Content
```

## CategoriesPage.razor

The CategoriesPage component displays a grid of all active categories and provides admin controls.

### Features

- **Category Grid:** Displays all active (non-archived) categories
- **Archive Button:** Admin-only action to archive a category
- **Confirmation Dialog:** Protects against accidental archiving
- **Read-only for Non-Admins:** Non-administrators can view but cannot take actions

### Admin Workflow

1. Navigate to the Categories page
2. Locate the category to archive
3. Click the **Archive** button
4. Confirm the action in the dialog
5. The category is marked as archived and removed from all issue creation forms

### Implementation Notes

- Archive confirmation uses a modal dialog to prevent accidental data loss
- Archived categories are excluded from the display grid to reduce clutter
- Only users with the Administrator role can see and use the Archive button
