# Statuses API & UI

## Overview

Statuses track the lifecycle of issues (e.g., "Open", "In Progress", "Closed", "On Hold"). Admins can manage statuses, including archiving (soft-deleting) them to retire statuses while preserving historical data.

## DELETE /api/v1/statuses/{id}

Archives (soft-deletes) a status. Only administrators can perform this action.

### Authorization

- **Required Role:** Administrator
- **Response:** `204 No Content` on success or `403 Forbidden` if user lacks permissions

### Behavior

When a status is archived:
- The status document is marked with `Archived = true`
- The `ArchivedBy` field is set to the current user's ID
- The `ArchivedAt` timestamp is recorded
- Archived statuses **do not appear** in issue status selectors or workflows
- Existing issues with archived statuses remain visible but cannot be transitioned to that status
- The status can be restored by a squad member with database access (manual intervention)

### Example Request

```
DELETE /api/v1/statuses/507f1f77bcf86cd799439011
Authorization: Bearer {token}
```

### Example Response

```
HTTP/1.1 204 No Content
```

## StatusesPage.razor

The StatusesPage component displays a grid of all active statuses and provides admin controls.

### Features

- **Status Grid:** Displays all active (non-archived) statuses
- **Archive Button:** Admin-only action to archive a status
- **Confirmation Dialog:** Protects against accidental archiving
- **Read-only for Non-Admins:** Non-administrators can view but cannot take actions

### Admin Workflow

1. Navigate to the Statuses page
2. Locate the status to archive
3. Click the **Archive** button
4. Confirm the action in the dialog
5. The status is marked as archived and removed from all issue workflow selectors

### Implementation Notes

- Archive confirmation uses a modal dialog to prevent accidental data loss
- Archived statuses are excluded from the display grid to reduce clutter
- Only users with the Administrator role can see and use the Archive button
- Status archiving mirrors category archiving for consistency
