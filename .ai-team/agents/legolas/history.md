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

### CI/CD Pipeline Design — Test Execution (2026-02-17)

#### Parallelization Strategy
- **5 independent test jobs** (Unit, Architecture, bUnit, Integration, Aspire) run simultaneously
- **Single shared build job** with NuGet cache reduces redundancy (~5-10 min)
- **Total execution time: ~10-12 minutes** (parallel much faster than sequential ~25 min)
- Safe to parallelize because test suites have no shared state; each job is idempotent

#### Coverage Gates & Reporting
- **Coverlet collector** enabled on Unit, bUnit, Integration, Aspire (XPlat Code Coverage format)
- **Architecture tests excluded** from coverage due to NetArchTest + Coverlet conflict (noted in original CI)
- **80% threshold** enforced as warning (can be made hard gate via branch protection)
- **ReportGenerator** aggregates `.cobertura.xml` files → HTML + Cobertura + JSON summary
- **Codecov integration** for historical tracking and badge generation

#### MongoDB in CI vs Local Dev
- **CI:** GitHub Actions service container (mongo:7.0) auto-provisioned with health checks
- **Local Dev:** Testcontainers or Docker Compose for developer flexibility
- **Service-based in CI** avoids Docker-in-Docker complexity; replicates production topology

#### Test Environment vs Production Configuration
- **CI env vars:** Dummy Auth0 values, test MongoDB connection string
- **Production:** Secrets stored in GitHub Secrets or Key Vault, rotated regularly
- **Aspire configuration:** Different manifests for dev (localhost), test (CI container), prod (cloud)

#### Artifact & Reporting Strategy
- **Per-job TRX uploads** named by test type (unit-test-results, integration-test-results, etc.)
- **EnricoMi action** parses TRX files and publishes to GitHub check suite (visible on PR)
- **Coverage reports** uploaded separately (HTML, Cobertura, JSON) for visibility
- **Job summary** auto-generated in GitHub Actions UI for quick overview

#### Error Handling & Observability
- Each job explicitly exits code 1 on test failure (hard failure propagation)
- Timeout protection: 10-20 min per job depending on type
- Coverage warnings (not failures) if <80% — allows CI to pass but alerts developers
- ReportGenerator handles missing files gracefully (warns instead of crashing)

#### Performance Considerations
- **NuGet cache hit:** 50-60% reduction in restore time (subsequent jobs benefit)
- **Build cache:** `dotnet build` is incremental; most builds skip unchanged projects
- **Timeout margins:** 15 min build + 10 min tests + 2 min overhead = 27 min total, well below 30 min runner default
- **Parallelism limit:** 5 jobs OK for standard GitHub runner; cost scales linearly

---

### E2E Test Removal — 2026-02-17

#### Why Removed
- **Aspire projects do not expose a static web endpoint** suitable for Playwright E2E testing
- **Service discovery is dynamic** — Aspire uses ephemeral ports and service mesh routing
- **E2E tests cannot reliably target the Blazor UI** without complex orchestration or external deployment
- **CI build time reduced** by ~5 minutes (removed Playwright browser install + E2E execution)

#### Changes Made
- **Removed `test-e2e` job** from `.github/workflows/test.yml` (lines 366-431)
- **Updated `report` job dependencies** — removed `test-e2e` from `needs` list
- **Updated job summary script** — removed `e2e_status` variable and E2E Tests line from output
- **Verified `squad-ci.yml`** — no E2E references found (already clean)

#### Remaining Test Jobs
1. `build` — Solution build with NuGet caching
2. `test-unit` — Unit tests with coverage
3. `test-architecture` — NetArchTest rules (no coverage)
4. `test-bunit` — Blazor component tests with coverage
5. `test-integration` — Integration tests with MongoDB service
6. `test-aspire` — Aspire orchestration tests with coverage
7. `coverage` — Aggregate coverage from all sources
8. `report` — Unified test result summary

#### Future E2E Strategy (if needed)
- Deploy Aspire app to temporary container/K8s environment in CI
- Wait for service readiness via health checks
- Run Playwright tests against deployed endpoints
- Teardown after test completion
- **Trade-off:** Adds 10-15 min to CI pipeline vs current in-process testing
