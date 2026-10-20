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

An issue management application built with modern architecture patterns and async/reactive workflows.
IssueManager demonstrates vertical slice architecture, CQRS, and MongoDB integration in a production-ready .NET application.

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

Features are organized as vertical slices—each slice owns its complete stack from API to UI. Commands handle writes, queries handle reads. MongoDB is our primary data store.
Aspire manages service topology and local development.

## Contributing

See [CONTRIBUTING.md](docs/CONTRIBUTING.md) and the [Code of Conduct](docs/CODE_OF_CONDUCT.md).
Branches, worktrees, commits, PRs, merging and releases follow [PROCESS.md](docs/PROCESS.md).

## License

See [LICENSE](LICENSE) for details.

## Releases

<!-- RELEASES_START -->

| Version | Date | Title | Blog post |
| ------- | ---- | ----- | --------- |
| [v0.0.80](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.80) | 2026-10-20 | chore: Re-apply the repo-ci-baseline Template | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-20-pr-318-chore-re-apply-the-repo-ci-baseline-template.md) |
| [v0.0.79](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.79) | 2026-10-18 | chore: Re-apply the repo-ci-baseline Template | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-18-pr-316-chore-re-apply-the-repo-ci-baseline-template.md) |
| [v0.0.78](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.78) | 2026-10-18 | chore(deps): Have Dependabot ignore claude-code-action | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-18-pr-313-chore-deps-have-dependabot-ignore-claude-code-action.md) |
| [v0.0.77](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.77) | 2026-10-18 | chore: Re-apply the repo-ci-baseline Template | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-18-pr-309-chore-re-apply-the-repo-ci-baseline-template.md) |
| [v0.0.76](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.76) | 2026-10-16 | build(deps): Update NuGet packages | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-16-pr-307-build-deps-update-nuget-packages.md) |
| [v0.0.75](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.75) | 2026-10-15 | chore: Re-apply the repo-ci-baseline Template | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-15-pr-305-chore-re-apply-the-repo-ci-baseline-template.md) |
| [v0.0.74](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.74) | 2026-10-10 | chore: Re-apply the repo-ci-baseline Template | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-10-pr-301-chore-re-apply-the-repo-ci-baseline-template.md) |
| [v0.0.73](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.73) | 2026-10-10 | chore: Re-apply the repo-ci-baseline Template | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-10-pr-299-chore-re-apply-the-repo-ci-baseline-template.md) |
| [v0.0.72](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.72) | 2026-10-10 | chore: Re-apply the repo-ci-baseline Template | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-10-pr-297-chore-re-apply-the-repo-ci-baseline-template.md) |
| [v0.0.71](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.71) | 2026-10-10 | chore: Re-apply the repo-ci-baseline Template | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-10-pr-295-chore-re-apply-the-repo-ci-baseline-template.md) |

<!-- RELEASES_END -->

[All releases →](https://github.com/mpaulosky/IssueManager/releases)
