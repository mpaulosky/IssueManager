# IssueManager

[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![xUnit Tests](https://img.shields.io/badge/Tests-xUnit-blueviolet?logo=github)](https://github.com/mpaulosky/IssueManager/actions/workflows/squad-ci.yml)

[![CI/CD](https://github.com/mpaulosky/IssueManager/actions/workflows/squad-ci.yml/badge.svg)](https://github.com/mpaulosky/IssueManager/actions/workflows/squad-ci.yml)
[![Test Suite](https://github.com/mpaulosky/IssueManager/actions/workflows/squad-test.yml/badge.svg)](https://github.com/mpaulosky/IssueManager/actions/workflows/squad-test.yml)

[![CodeCov Coverage](https://codecov.io/gh/mpaulosky/IssueManager/branch/main/graph/badge.svg)](https://codecov.io/gh/mpaulosky/IssueManager)
[![Coverage Trend](https://img.shields.io/badge/Coverage-Trend-blue?logo=codecov)](https://codecov.io/gh/mpaulosky/IssueManager/commits/main)
[![Open Issues](https://img.shields.io/github/issues/mpaulosky/IssueManager?color=0366d6)](https://github.com/mpaulosky/IssueManager/issues?q=is%3Aopen+is%3Aissue)
[![Closed Issues](https://img.shields.io/github/issues-closed/mpaulosky/IssueManager?color=6f42c1)](https://github.com/mpaulosky/IssueManager/issues?q=is%3Aclosed+is%3Aissue)
[![Open PRs](https://img.shields.io/github/issues-pr/mpaulosky/IssueManager?color=28a745)](https://github.com/mpaulosky/IssueManager/pulls?q=is%3Aopen+is%3Apr)
[![Merged PRs](https://img.shields.io/github/issues-pr-closed/mpaulosky/IssueManager?color=6f42c1)](https://github.com/mpaulosky/IssueManager/pulls?q=is%3Amerged+is%3Apr)

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

See [`.ai-team/`](.ai-team/) for team structure and [`.github/CODE_OF_CONDUCT.md`](.github/CODE_OF_CONDUCT.md) for community guidelines. We follow the squad-based development model with defined ownership and approval gates.

## License

See [LICENSE](LICENSE) for details.
