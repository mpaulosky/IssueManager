# IssueManager

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
