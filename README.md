# IssueManager

[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![xUnit Tests](https://img.shields.io/badge/Tests-xUnit-blueviolet?logo=github)](https://github.com/mpaulosky/IssueManager/actions/workflows/ci.yml)
[![Latest Release](https://img.shields.io/github/v/release/mpaulosky/IssueManager?logo=github&color=blue&label=Release)](https://github.com/mpaulosky/IssueManager/releases/latest)

[![CI/CD](https://github.com/mpaulosky/IssueManager/actions/workflows/ci.yml/badge.svg)](https://github.com/mpaulosky/IssueManager/actions/workflows/ci.yml)

[![CodeCov Coverage](https://codecov.io/gh/mpaulosky/IssueManager/branch/main/graph/badge.svg)](https://codecov.io/gh/mpaulosky/IssueManager)
[![Coverage Trend](https://img.shields.io/badge/Coverage-Trend-blue?logo=codecov)](https://codecov.io/gh/mpaulosky/IssueManager/commits/main)
[![Coverage Gate](https://img.shields.io/badge/Coverage%20Gate->80%25-brightgreen?logo=codecov)](https://github.com/mpaulosky/IssueManager/actions/workflows/ci.yml)

[![Open Issues](https://img.shields.io/github/issues/mpaulosky/IssueManager?color=0366d6)](https://github.com/mpaulosky/IssueManager/issues?q=is%3Aopen+is%3Aissue)
[![Closed Issues](https://img.shields.io/github/issues-closed/mpaulosky/IssueManager?color=6f42c1)](https://github.com/mpaulosky/IssueManager/issues?q=is%3Aclosed+is%3Aissue)
[![Open PRs](https://img.shields.io/github/issues-pr/mpaulosky/IssueManager?color=28a745)](https://github.com/mpaulosky/IssueManager/pulls?q=is%3Aopen+is%3Apr)
[![Closed PRs](https://img.shields.io/github/issues-pr-closed/mpaulosky/IssueManager?color=6f42c1)](https://github.com/mpaulosky/IssueManager/pulls?q=is%3Aclosed+is%3Apr)

An issue management application built with modern architecture patterns and async/reactive workflows. IssueManager demonstrates vertical slice architecture, CQRS, and MongoDB integration in a production-ready .NET application.

## Quick Start

1. **Prerequisites:** .NET 10 SDK, Docker (for MongoDB)
2. **Clone & Restore:**
   ```bash
   git clone https://github.com/mpaulosky/IssueManager.git
   cd IssueManager
   dotnet restore
   ```
3. **Run:** `dotnet run --project AppHost` (Aspire orchestration)
4. **Open:** `https://localhost:5001` (Blazor UI)

## Tech Stack

- **.NET 10** — Latest stable framework
- **Aspire** — Service orchestration & local dev
- **Blazor** — Interactive web UI (server-side rendering)
- **MongoDB.EntityFramework** — Data access
- **CQRS** — Command/query separation
- **Vertical Slice Architecture** — Feature-based organization

## Architecture

Features are organized as vertical slices—each slice owns its complete stack from API to UI. Commands handle writes, queries handle reads. MongoDB is our primary data store. Aspire manages service topology and local development.

## Contributing

See [`.github/CODE_OF_CONDUCT.md`](.github/CODE_OF_CONDUCT.md) for community guidelines. Work happens on feature/fix/chore branches with PR review before merging to `main`.

## License

See [LICENSE](LICENSE) for details.

## Dev Blog

<!-- BLOG_START -->
| Date | Title | Tags |
|------|-------|------|
| 2026-08-30 | [refactor: hoist paginated GetAllAsync into MongoRepository base](docs/blogs/2026-08-30-pr-185-refactor-hoist-paginated-getallasync-into-mongorepository-base.md) | release,automation |
| 2026-08-30 | [refactor: collapse Category/Status CRUD handlers into generic TaxonomyCrudHandler](docs/blogs/2026-08-30-pr-183-refactor-collapse-category-status-crud-handlers-into-generic-taxonomycrudhandler.md) | release,automation |
| 2026-08-29 | [docs: add commit/release blog-post scripts and docs index page](docs/blogs/2026-08-29-pr-179-docs-add-commit-release-blog-post-scripts-and-docs-index-page.md) | release,automation |
<!-- BLOG_END -->
