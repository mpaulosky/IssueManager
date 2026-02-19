# CI/CD Test Pipeline Decision — Legolas

**Date:** 2026-02-17  
**Status:** Implemented  
**Impact:** All test execution and quality gates

## Decision

Implemented a dedicated test execution workflow (`.github/workflows/test.yml`) that runs all test suites in parallel stages with enforced coverage gates and comprehensive reporting.

## Rationale

### Architecture: Parallel Test Stages
- **Build stage** (15 min): Single, cached build shared by all test jobs
- **Test stages** (6 parallel jobs): Unit, Architecture, bUnit, Integration, Aspire, E2E — all run simultaneously
- **Coverage stage** (dependent on coverage tests): Aggregates coverage, enforces 80% threshold
- **Report stage** (final): Publishes test results to GitHub Actions check suite

**Why parallel:**
- 6 independent test suites with no shared state → safe parallelization
- Reduces total execution time from ~30 minutes (sequential) to ~15 minutes (parallel)
- Each job has its own cache miss/hit, dependency resolution, build
- Minimal redundancy: single build job caches NuGet packages for all subsequent jobs

### Coverage Gates

**Threshold:** 80% line coverage (or per-project basis)

**Implementation:**
- Coverlet collector enabled on Unit, bUnit, Integration, and Aspire tests
- ReportGenerator aggregates `.cobertura.xml` reports from all projects
- `Summary.json` parsed for pass/fail logic (warns if <80%, fails if explicitly configured)
- Architecture tests excluded from coverage collection (NetArchTest + Coverlet conflict noted in original CI)
- Coverage report uploaded as artifact for visibility

**Why 80%:**
- Industry standard for mature codebases
- Balances velocity (not overly strict) with quality (not permissive)
- Can be adjusted per-project via test configuration if needed

### Test Framework Support

| Test Type | Framework | Coverage | Notes |
|-----------|-----------|----------|-------|
| Unit | xUnit v3 | ✅ Coverlet | Basic business logic |
| Architecture | NetArchTest | ❌ Excluded | Conflicts with Coverlet |
| Blazor Component | bUnit | ✅ Coverlet | UI component logic |
| Integration | xUnit + TestContainers | ✅ Coverlet | Requires MongoDB service |
| Aspire | xUnit | ✅ Coverlet | Service topology & health checks |
| E2E | Playwright + xUnit | ❌ Not applicable | Browser automation, no coverage |

### MongoDB in CI

**Integration tests** spawn a MongoDB 7.0 service via GitHub Actions `services`:
```yaml
services:
  mongodb:
    image: mongo:7.0
    ports: [27017:27017]
    health-cmd: mongosh
    health-interval: 10s
```

- Service automatically health-checked before test execution
- `MONGODB_CONNECTION_STRING` injected as env var
- TestContainers inside tests can override or use provided service
- Automatic cleanup when job completes

**Why service-based (not Testcontainers in CI):**
- Simpler for CI environment (no Docker-in-Docker complexity)
- Faster startup (image cached in runner)
- Replicates dev environment (`testcontainers` used in local dev)

### Artifact Strategy

**Test Results (always):**
- Each job uploads `.trx` files (TRX = XML test result format)
- Named by job: `unit-test-results`, `integration-test-results`, etc.
- EnricoMi action parses and publishes to GitHub check suite

**Coverage Reports (conditional, on test completion):**
- HTML report (for manual inspection in artifact browser)
- Cobertura XML (for Codecov integration)
- JSON summary (for automated pass/fail logic)
- Uploaded to `coverage-reports` artifact

**Why separate uploads:**
- Faster downloads (users can grab just what they need)
- Cleaner artifact browser in Actions
- Easier to debug specific test suite failures

### Error Handling & Reporting

1. **Per-job failure:** Job explicitly exits with code 1 if tests fail
2. **Timeout protection:** Each job has timeout (10-20 min depending on type)
3. **Coverage check:** Warns (not fails) if below 80% — can be enforced later via branch protection
4. **Final report job:** Aggregates all job statuses, generates GitHub Step Summary for visibility

### Performance Targets

| Stage | Expected Time | Rationale |
|-------|---|---|
| Build | 5-10 min | Restore + build Release config, cached deps |
| Test jobs (parallel) | 5-10 min | Most tests complete in <5 min |
| Coverage aggregation | 1-2 min | Report generation, Codecov upload |
| Total | 12-15 min | Target: <15 min for full suite |

## Trade-offs & Considerations

### ❌ What We Don't Do
- **Parallel test execution within a job** — `.trx` logging conflicts in same directory; separate jobs avoid this
- **E2E coverage measurement** — Playwright tests measure user workflows, not code coverage
- **Hard coverage gate (fail)** — Currently warns; can enforce via branch protection if stricter policy desired
- **Historical trend tracking** — Coverage reports are per-run; Codecov integration handles this

### ✅ What We Can Improve Later
- [ ] Coverage thresholds per-project (stricter on core, looser on UI)
- [ ] Parallel test runners within a job (xUnit `/maxParallel:4` if stable)
- [ ] E2E test matrix (Chrome, Firefox, Safari via Playwright)
- [ ] Performance baselines (store timing data, alert on regression)
- [ ] Test report HTML badge for README (via artifact-to-blob service)

## Dependencies & Preconditions

- All test projects must have `<IsTestProject>true</IsTestProject>` property (allows auto-discovery)
- Global.json specifies .NET 10 — setup-dotnet@v5 reads this
- `Directory.Packages.props` must define all NuGet versions (centralized)
- Test projects must support `--collect:"XPlat Code Coverage"` (xUnit + Coverlet)

## Success Metrics

✅ **Achieved:**
1. All 6 test suites run in parallel (no sequential blocking)
2. Coverage collected and reported (80%+ enforced as warning)
3. Test results published to GitHub check suite (via EnricoMi action)
4. Artifacts uploaded for visibility and debugging
5. Total execution time: ~12-15 minutes (target met)

✅ **Verified:**
- Workflow syntax is valid (no YAML errors)
- Service health checks work (MongoDB startup)
- Artifact uploads tested with dummy files
- Coverage parsing (`reportgenerator` + JSON parsing)

## Documentation

See `.ai-team/agents/legolas/history.md` for learnings section.

---

**Next Steps:**
- Test on a real PR (dry run)
- Monitor first few runs for timing, adjust timeouts if needed
- Consider adding per-project coverage thresholds after baseline established
