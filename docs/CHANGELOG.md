# Changelog

All notable changes to IssueManager are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Sprint 3

#### Added

- **Issue Filters & Search** (#116)
  - `GET /api/v1/issues` now supports `page`, `pageSize`, `searchTerm`, `authorName`, `statusName`, and `categoryName` query parameters
  - IssuesPage component wires all filters to the API call for real-time filtering
  - Search term performs regex-based matching on issue titles and descriptions (case-insensitive)

- **Category Archive API** (#120)
  - `DELETE /api/v1/categories/{id}` endpoint for admin-only category archiving (soft-delete)
  - Archived categories are marked with `Archived=true` and `ArchivedBy` timestamp
  - Archived categories no longer appear in issue creation forms
  - Preserves historical data while removing active usage

- **Status Archive API** (#121)
  - `DELETE /api/v1/statuses/{id}` endpoint for admin-only status archiving (soft-delete)
  - Archived statuses are marked with `Archived=true` and `ArchivedBy` timestamp
  - Archived statuses no longer appear in issue workflow selectors
  - Preserves historical data while removing active usage

- **Category Archive UI** (#124)
  - Admin-only Archive button in CategoriesPage grid
  - Confirmation dialog to prevent accidental archiving
  - Archived categories excluded from display

- **Status Archive UI** (#123)
  - Admin-only Archive button in StatusesPage grid
  - Confirmation dialog to prevent accidental archiving
  - Archived statuses excluded from display

#### Documentation

- Added comprehensive API documentation for issue filtering
- Added soft-delete behavior documentation for categories and statuses
- Added admin UI workflow documentation for archiving operations
