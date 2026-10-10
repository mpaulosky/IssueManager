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
| [v0.0.74](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.74) | 2026-10-10 | chore: Re-apply the repo-ci-baseline Template | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-10-pr-301-chore-re-apply-the-repo-ci-baseline-template.md) |
| [v0.0.73](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.73) | 2026-10-10 | chore: Re-apply the repo-ci-baseline Template | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-10-pr-299-chore-re-apply-the-repo-ci-baseline-template.md) |
| [v0.0.72](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.72) | 2026-10-10 | chore: Re-apply the repo-ci-baseline Template | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-10-pr-297-chore-re-apply-the-repo-ci-baseline-template.md) |
| [v0.0.71](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.71) | 2026-10-10 | chore: Re-apply the repo-ci-baseline Template | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-10-pr-295-chore-re-apply-the-repo-ci-baseline-template.md) |
| [v0.0.70](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.70) | 2026-10-10 | chore: Re-apply the repo-ci-baseline Template | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-10-pr-293-chore-re-apply-the-repo-ci-baseline-template.md) |
| [v0.0.69](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.69) | 2026-10-10 | chore: Re-apply the repo-ci-baseline Template | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-10-pr-291-chore-re-apply-the-repo-ci-baseline-template.md) |
| [v0.0.68](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.68) | 2026-10-09 | chore: Re-apply the repo-ci-baseline Template | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-09-pr-289-chore-re-apply-the-repo-ci-baseline-template.md) |
| [v0.0.67](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.67) | 2026-10-09 | fix: Publish each Sandcastle issue as its own PR, and fix the other #276 review findings | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-09-pr-285-fix-publish-each-sandcastle-issue-as-its-own-pr-and-fix-the-other-276-review-findings.md) |
| [v0.0.66](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.66) | 2026-10-09 | fix: Follow up on Sandcastle check review findings from #281 | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-09-pr-286-fix-follow-up-on-sandcastle-check-review-findings-from-281.md) |
| [v0.0.65](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.65) | 2026-10-09 | chore: Give Sandcastle a Docker-free check script | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-09-pr-281-chore-give-sandcastle-a-docker-free-check-script.md) |

<!-- RELEASES_END -->

[All releases →](https://github.com/mpaulosky/IssueManager/releases)
