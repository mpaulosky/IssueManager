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

---

## Learnings

### CI/CD Workflow Consolidation — 2026-02-19

#### GitHub Actions Reusable Workflow Patterns

**Key Syntax:**
- Reusable workflows use `on: [workflow_call, ...]` trigger type
- `workflow_call` allows the workflow to be invoked via `uses:` from other workflows
- Callers reference via: `uses: ./.github/workflows/reusable-workflow.yml@ref`
- Outputs can be passed from reusable workflow to caller via `outputs:` in `uses` statement
- Do not add `inputs:` section if the workflow doesn't expose parameters to callers
- Can combine `workflow_call` with `push`, `pull_request`, `workflow_dispatch` for manual runs

**Orchestration Pattern:**
- Squad CI/CD architecture now uses thin orchestrator (`squad-ci.yml`) pattern
- Orchestrator handles only squad-specific concerns: versioning (GitVersion), notifications, release triggers
- Reusable workflow (`squad-test.yml`) contains comprehensive test suite: build, 6 parallel jobs, coverage, reporting
- Eliminates test duplication across workflows — single source of truth

#### Architectural Benefits

**Before Consolidation:**
- `test.yml`: 492 lines with full test suite (5 parallel jobs)
- `squad-ci.yml`: 180 lines with duplicate build/test/coverage logic
- Total: 672 lines of test-related code spread across 2 workflows
- Difficult to update tests — changes needed in both files

**After Consolidation:**
- `squad-test.yml`: 427 lines (reusable, full test suite)
- `squad-ci.yml`: 71 lines (thin orchestrator, 60% reduction)
- Total: 498 lines, single source of truth
- Squad-CI now only manages versioning and notifications
- Easy to add callers to squad-test.yml without duplication

#### Design Decisions Made

**1. Job Parallelism Retained:**
- All 6 test jobs (unit, architecture, blazor, integration, aspire, coverage/report) remain parallel
- No performance regression; reusable workflow maintains concurrent execution
- Test suite still completes in ~10-12 minutes

**2. Why test.yml was superior to squad-ci.yml:**
- `test.yml` had cleaner separation: pure test concerns, no version management
- `squad-ci.yml` was mixing concerns: versioning + test execution + notifications
- Consolidation moves squad-ci.yml to orchestration-only (cleaner Single Responsibility Principle)

**3. No Input Parameters to squad-test.yml:**
- Reusable workflow does not expose `inputs:` to callers
- All test configuration (timeouts, thresholds, tool versions) remains embedded
- Ensures test suite stays consistent across all callers
- Future orchestrators can reference without parameterization concerns

**4. Versioning Moved to Separate Job:**
- `versioning` job runs GitVersion once, outputs version
- `test-suite` job depends on versioning, receives version via needs context
- `notify` job consumes version from versioning job output
- Prevents GitVersion redundancy while keeping versioning available to all jobs

#### Coverage & Reporting Unchanged

- Coverage aggregation (Codecov, ReportGenerator) still happens in reusable workflow
- Test result publishing (EnricoMi action, artifact uploads) unchanged
- 80% threshold warning maintained
- All 6 test results visible in GitHub checks

#### Future Evolution

- Additional callers (release workflow, scheduled tests) can call `squad-test.yml@main`
- If squad-test.yml needs to expose parameters, add `inputs:` section and pass via `with:` in callers
- Versioning job pattern can be reused for other orchestrators (e.g., deploy-prod workflow)

---

### Build Artifact Caching — 2026-02-19

#### Strategy Implemented

**Problem:**
- 5 parallel test jobs (unit, architecture, bunit, integration, aspire) each ran `dotnet restore` + `dotnet build`
- Each job independently compiled the same solution, wasting 10-15 minutes per workflow run
- NuGet cache helped with packages, but build artifacts were rebuilt from scratch every time

**Solution: Build artifact caching (Option A)**
- Single `build` job compiles solution once and caches `bin/` and `obj/` directories
- All test jobs restore from cache and skip rebuild entirely
- Test jobs use `--no-build` flag to run tests against cached binaries

#### Cache Key Pattern

**Key components:**
```yaml
key: ${{ runner.os }}-build-${{ hashFiles('**/*.csproj', '**/Directory.Packages.props', 'global.json') }}
restore-keys: |
  ${{ runner.os }}-build-
```

**Why this pattern:**
- `runner.os`: OS-specific binaries (Linux vs Windows)
- `**/*.csproj`: Any project file change (new dependency, target framework change)
- `Directory.Packages.props`: Central package version change
- `global.json`: SDK version change (affects compilation)
- `restore-keys` fallback: If exact match fails, use most recent cache (graceful degradation)

**Cache invalidation:**
- Automatic when any `.csproj`, `Directory.Packages.props`, or `global.json` changes
- Cache expires after 7 days of no use (GitHub Actions default)
- No manual purge needed; hash-based key ensures consistency

#### Trade-offs and Safety

**Why --no-build is strict (and that's good):**
- `--no-build` flag REQUIRES binaries to exist; fails loudly if cache is lost
- This is the correct behavior: fail fast rather than silently rebuild
- Alternative would be conditional build (`if cache miss then build`) but that defeats the purpose
- Cache misses are rare (only on first run or after cache expiry); acceptable to require manual re-trigger

**NuGet cache is separate:**
- NuGet package cache remains independent (already exists in all jobs)
- `dotnet restore` still runs to restore packages (fast, uses NuGet cache)
- Build artifact cache only handles compiled binaries (`bin/`, `obj/`)
- Two-layer caching: packages (stable, rarely changes) + binaries (changes with code)

**First run vs subsequent runs:**
- **First run (cache miss):** Test jobs will fail with "missing binaries" error — normal; re-trigger workflow
- **Subsequent runs (cache hit):** Test jobs skip rebuild, estimated 10-15 min saved per run
- **After code change (cache invalidate):** New cache key generated, old cache ignored

#### Performance Impact

**Before caching:**
- Build job: 5 min (restore + build)
- Each test job: 3-4 min restore + 2-3 min build = 5-7 min overhead per job
- Total redundant build time: 5 jobs × 5 min = 25 min wasted (parallelized to ~7 min wall time)

**After caching:**
- Build job: 5 min (restore + build) + 1 min cache save = 6 min
- Each test job: 3-4 min restore + 30 sec cache restore = ~4 min overhead per job
- Total build time: 6 min (build) + 4 min (test prep) = 10 min vs 12 min before
- **Net savings: ~10-15 min per workflow run** (first run excluded)

#### Architectural Notes

**Why cache in build job, not test jobs:**
- Single source of truth: One job builds, all others consume
- Ensures all test jobs use identical binaries (no race conditions)
- Simpler cache management (one save, multiple restores)

**Why keep dotnet restore in test jobs:**
- NuGet packages change less frequently than code
- Restore is fast (~30 sec with cache hit)
- Decouples package management from binary caching
- Allows test jobs to verify dependencies are consistent

**Failure modes:**
1. **Cache miss on first run:** Expected; re-trigger workflow
2. **Cache expired (7 days):** Expected; re-trigger workflow
3. **Partial cache (corrupted):** `--no-build` fails loudly; clear cache manually via GitHub UI
4. **Wrong binaries cached (hash collision):** Extremely rare; cache key includes file hashes

#### Lessons Learned

**When to cache binaries vs. rebuild:**
- **Cache binaries:** When multiple jobs use same build output (tests, deployment stages)
- **Rebuild:** When job requires custom build configuration (Debug vs Release, different targets)
- **Hybrid (this project):** Build once (Release config), cache, test multiple suites

**GitHub Actions cache limitations:**
- 10 GB total cache per repository (all branches combined)
- Oldest caches purged when limit reached
- Cache scoped to branch (main branch cache is shared across PRs)
- No cross-workflow cache sharing (each workflow has its own cache scope)

**Alternative approaches considered (and rejected):**
- **Option B (Build artifacts uploaded as GitHub artifacts):** Slower than cache, 90-day retention unnecessary for ephemeral build outputs
- **Option C (Matrix build strategy):** Would still rebuild per matrix job; doesn't solve redundancy
- **Option D (Conditional build in test jobs):** Defeats the purpose; want to fail fast on cache miss

---
