# History — Legolas

## Project Learnings (from init)

### IssueManager Project — Started 2026-02-17

**Tech Stack:**
- Aspire (.NET orchestration)
- MongoDB (data store)
- Docker (containerization)
- GitHub Actions (CI/CD)

**Local development:**
- Aspire Dashboard for local debugging and service monitoring
- Docker Compose for MongoDB and other services
- Hot reload for C# and Blazor development

**MongoDB configuration:**
- Connection string management (dev, test, prod)
- Index strategy for common queries (Issue list, search, filtering)
- Schema versioning strategy

**CI/CD:**
- Build pipeline: restore, build, test, publish
- Deploy pipeline: containerize, push, deploy to hosting
- Observability: structured logging, metrics collection

---

## Learnings

### .gitignore Security Best Practices
- Always exclude `.env` and local config files — these contain secrets (DB passwords, API keys, tokens)
- .NET projects generate large build artifacts (`bin/`, `obj/`) — exclude to keep repo lean
- IDE user files (`.csproj.user`, Rider `.idea/`) are developer-specific and shouldn't be version controlled
- MongoDB local data (`.mongo/`, `mongodb-data/`) must be excluded to prevent data leaks
- Aspire debug manifests should be excluded but `.ai-team/` and `.github/` must be version controlled
- Test coverage reports and logs are transient — exclude to reduce noise
- For Blazor + Aspire projects, also exclude `appsettings.Development.local.json` to allow local overrides without commits
