# CI/CD Test Pipeline — Quick Reference

## What Was Built

**File:** `.github/workflows/test.yml`

A comprehensive GitHub Actions workflow that executes all test suites in parallel with coverage gates and quality reporting.

## Test Suite Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│ Checkout & Setup                                                 │
│ └─ .NET 10, NuGet cache, global.json                            │
└──────────────────────┬──────────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────────┐
│ Build (15 min)                                                   │
│ └─ Restore & Build (Release config) → cached for all test jobs  │
└──────────────────────┬──────────────────────────────────────────┘
                       │
        ┌──────────────┼──────────────┐
        │              │              │
        ▼              ▼              ▼
┌────────────────┐ ┌──────────────┐ ┌──────────────┐
│  Unit Tests    │ │ Architecture │ │ bUnit Tests  │
│  (xUnit + Cov) │ │ (NetArchTest)│ │ (Coverage)   │
│  ~5 min        │ │  ~3 min      │ │  ~5 min      │
└────────────────┘ └──────────────┘ └──────────────┘
        │              │              │
        ▼              ▼              ▼
┌────────────────┐ ┌──────────────┐ ┌──────────────┐
│  Integration   │ │ Aspire Tests │ │  E2E Tests   │
│  (MongoDB srv) │ │ (Topology)   │ │ (Playwright) │
│  ~8 min        │ │  ~8 min      │ │  ~10 min     │
└────────────────┘ └──────────────┘ └──────────────┘
        │              │              │
        └──────────────┼──────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────────┐
│ Coverage Analysis (2 min)                                        │
│ └─ ReportGenerator aggregates .cobertura.xml → 80% gate         │
└──────────────────────┬──────────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────────┐
│ Report & Publish (1 min)                                        │
│ └─ TRX parsing → GitHub check suite + Step Summary              │
└─────────────────────────────────────────────────────────────────┘

Total: ~12-15 minutes (parallel >> sequential)
```

## Test Suites

| Suite | Framework | Coverage | Trigger | Notes |
|-------|-----------|----------|---------|-------|
| **Unit** | xUnit v3 | ✅ Yes | Always | Business logic, domain layer |
| **Architecture** | NetArchTest | ❌ Excluded | Always | Layer constraints, dependency rules |
| **Blazor** | bUnit | ✅ Yes | Always | Component rendering, interactions |
| **Integration** | xUnit + TestContainers | ✅ Yes | Always | MongoDB required (auto-provisioned) |
| **Aspire** | xUnit | ✅ Yes | Always | Service topology, health checks |
| **E2E** | Playwright + xUnit | ❌ N/A | Always | Browser automation, user workflows |

## Coverage Gates

- **Threshold:** 80% line coverage (warning if below)
- **Collected from:** Unit, bUnit, Integration, Aspire tests
- **Excluded:** Architecture (conflicts), E2E (not applicable)
- **Tool:** Coverlet collector + ReportGenerator
- **Reporting:** Codecov integration for historical trends

## MongoDB in CI

Integration tests require MongoDB:

```yaml
services:
  mongodb:
    image: mongo:7.0
    ports: [27017:27017]
    health-cmd: mongosh
```

- Automatically started before job, health-checked
- `MONGODB_CONNECTION_STRING` env var injected
- Cleaned up after job completes

## Artifacts

**Test Results** (always uploaded):
- `unit-test-results/unit.trx`
- `architecture-test-results/architecture.trx`
- `bunit-test-results/bunit.trx`
- `integration-test-results/integration.trx`
- `aspire-test-results/aspire.trx`
- `e2e-test-results/e2e.trx`

**Coverage Reports** (uploaded after coverage analysis):
- `coverage-reports/index.html` — Human-readable coverage report
- `coverage-reports/Cobertura.xml` — Machine-readable (Codecov)
- `coverage-reports/Summary.json` — Coverage metrics

## Triggers

Workflow runs on:
- `push` to `main` or `squad/*` branches
- `pull_request` to any branch
- Manual trigger via `workflow_dispatch`

## Local Testing

To replicate CI locally:

```bash
# Restore & build
dotnet restore
dotnet build --configuration Release

# Run tests (single or all)
dotnet test tests/Unit --configuration Release
dotnet test tests/Integration --configuration Release --collect:"XPlat Code Coverage"

# Coverage report (requires ReportGenerator)
dotnet tool install -g reportgenerator
reportgenerator -reports:coverage/**/*.opencover.xml -targetdir:coverage -reporttypes:Html
```

## Performance Targets

| Phase | Time | Acceptable |
|-------|------|-----------|
| Build | 5-10 min | ✅ Cached deps |
| Tests (parallel) | 5-10 min | ✅ All suites concurrent |
| Coverage | 1-2 min | ✅ Report generation |
| Report | <1 min | ✅ Summary + publish |
| **Total** | **12-15 min** | ✅ **Well under 30 min default** |

## Failure Scenarios

| Scenario | Behavior |
|----------|----------|
| Build fails | All test jobs skipped (dependency) |
| Test job fails | Job reports failure, continues to coverage |
| Coverage <80% | Warning message, workflow continues to pass |
| Service unavailable | Job timeout (15-20 min), explicit failure |
| Codecov unavailable | Warning, coverage report still generated |

## Configuration

All configuration is in `global.json` and `Directory.Packages.props`:

**global.json** — Specifies .NET 10 SDK version  
**Directory.Packages.props** — Centralized NuGet versions

No per-project version specifications; workflow reads from these files.

## Next Steps

1. **Monitor first runs** — Check timing, adjust timeouts if needed
2. **Set branch protection** — Require "Test Results Summary" check to pass on PRs
3. **Configure Codecov** — Link repository for coverage trend tracking
4. **Per-project thresholds** — Adjust coverage gates for specific test projects if needed
5. **E2E matrix expansion** — Add browser matrix (Chrome, Firefox) when ready

---

**Decision document:** `.ai-team/decisions/inbox/legolas-cicd-pipeline.md`  
**Learnings:** `.ai-team/agents/legolas/history.md`
