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
| [v0.0.58](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.58) | 2026-10-08 | chore: Re-apply the repo-ci-baseline Template | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-08-pr-268-chore-re-apply-the-repo-ci-baseline-template.md) |
| [v0.0.57](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.57) | 2026-10-08 | chore: Re-apply the repo-ci-baseline Template | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-08-pr-266-chore-re-apply-the-repo-ci-baseline-template.md) |
| [v0.0.56](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.56) | 2026-10-08 | chore: Re-apply the repo-ci-baseline Template | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-08-pr-264-chore-re-apply-the-repo-ci-baseline-template.md) |
| [v0.0.55](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.55) | 2026-10-08 | chore: Re-apply the repo-ci-baseline Template | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-08-pr-262-chore-re-apply-the-repo-ci-baseline-template.md) |
| [v0.0.54](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.54) | 2026-10-07 | chore: Re-apply the repo-ci-baseline Template | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-07-pr-260-chore-re-apply-the-repo-ci-baseline-template.md) |
| [v0.0.53](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.53) | 2026-10-07 | chore: Re-apply the repo-ci-baseline Template | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-07-pr-257-chore-re-apply-the-repo-ci-baseline-template.md) |
| [v0.0.52](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.52) | 2026-10-05 | chore: Re-apply the repo-ci-baseline Template for the release-post fixes | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-05-pr-255-chore-re-apply-the-repo-ci-baseline-template-for-the-release-post-fixes.md) |
| [v0.0.51](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.51) | 2026-10-05 | chore: Use pnpm instead of npm and npx in the gate, hooks and Squad skill | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-05-pr-252-chore-use-pnpm-instead-of-npm-and-npx-in-the-gate-hooks-and-squad-skill.md) |
| [v0.0.50](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.50) | 2026-10-05 | chore: Re-apply the repo-ci-baseline Template | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-05-pr-251-chore-re-apply-the-repo-ci-baseline-template.md) |
| [v0.0.49](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.49) | 2026-10-05 | fix: Make the 11 skipped E2E tests pass | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-05-pr-249-fix-make-the-11-skipped-e2e-tests-pass.md) |

<!-- RELEASES_END -->

[All releases →](https://github.com/mpaulosky/IssueManager/releases)
