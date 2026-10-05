# IssueManager

[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](../LICENSE)
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

See [CONTRIBUTING.md](CONTRIBUTING.md) and the [Code of Conduct](CODE_OF_CONDUCT.md).
Branches, worktrees, commits, PRs, merging and releases follow [PROCESS.md](PROCESS.md).

## License

See [LICENSE](../LICENSE) for details.

## Releases

<!-- RELEASES_START -->

| Version | Date | Title | Blog post |
| ------- | ---- | ----- | --------- |
| [v0.0.48](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.48) | 2026-10-05 | fix: Start the E2E host so its Playwright tests run instead of skipping | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-05-pr-247-fix-start-the-e2e-host-so-its-playwright-tests-run-instead-of-skipping.md) |
| [v0.0.47](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.47) | 2026-10-05 | ci: Standardize on the repo-ci-baseline Template | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-05-pr-241-ci-standardize-on-the-repo-ci-baseline-template.md) |
| [v0.0.46](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.46) | 2026-10-02 | chore: drop leaked Context7 MCP key and stale security policies | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-10-02-pr-237-chore-drop-leaked-context7-mcp-key-and-stale-security-policies.md) |
| [v0.0.45](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.45) | 2026-09-30 | ci: Merge a PR only after Copilot's review and resolved threads | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-09-30-pr-235-ci-merge-a-pr-only-after-copilot-s-review-and-resolved-threads.md) |
| [v0.0.44](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.44) | 2026-09-30 | docs: Point the rest of CONTRIBUTING.md at main | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-09-30-pr-233-docs-point-the-rest-of-contributing-md-at-main.md) |
| [v0.0.43](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.43) | 2026-09-30 | ci(hooks): Adopt the shared branch-name standard | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-09-30-pr-231-ci-hooks-adopt-the-shared-branch-name-standard.md) |
| [v0.0.42](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.42) | 2026-09-30 | ci(hooks): Lint the staged Markdown, not the working copy | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-09-30-pr-228-ci-hooks-lint-the-staged-markdown-not-the-working-copy.md) |
| [v0.0.41](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.41) | 2026-09-29 | build: write a single-document pnpm lockfile | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-09-29-pr-225-build-write-a-single-document-pnpm-lockfile.md) |
| [v0.0.40](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.40) | 2026-09-29 | build: switch the web project from npm to pnpm | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-09-29-pr-222-build-switch-the-web-project-from-npm-to-pnpm.md) |
| [v0.0.39](https://github.com/mpaulosky/IssueManager/releases/tag/v0.0.39) | 2026-09-28 | chore(web): Stop committing the generated app.css | [Post](https://github.com/mpaulosky/IssueManager/blob/main/docs/blogs/2026-09-28-pr-220-chore-web-stop-committing-the-generated-app-css.md) |

<!-- RELEASES_END -->

[All releases →](https://github.com/mpaulosky/IssueManager/releases)
