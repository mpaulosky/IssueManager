# SKILL: .NET CI/CD Workflow Review

**Domain:** DevOps / Infrastructure  
**Owner:** Legolas  
**Created:** 2026-02-20  
**Status:** Active  

## Purpose

Systematic review methodology for GitHub Actions workflows in .NET projects to ensure SDK compatibility, proper tooling, coverage integration, and optimal job sequencing.

## When to Use This Skill

- Reviewing or auditing existing CI/CD workflows in .NET projects
- Validating workflow changes after SDK upgrades or tool updates
- Ensuring workflows follow .NET best practices
- Debugging CI/CD failures in .NET projects
- Onboarding new team members to CI/CD architecture

## Review Checklist

### 1. Workflow Triggers and Event Filters

**Check:**
- [ ] `push` events to main/primary branches
- [ ] `pull_request` events for PR validation
- [ ] `workflow_dispatch` for manual runs (optional)
- [ ] Concurrency control (`cancel-in-progress`) to prevent duplicate runs

**Validation:**
```yaml
on:
  push:
    branches: [main]
  pull_request:
    branches: ["**", "!main"]
  workflow_dispatch:

concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true
```

---

### 2. .NET SDK Setup

**Check:**
- [ ] `global.json` exists at repository root
- [ ] Workflow uses `actions/setup-dotnet@v5` (latest stable)
- [ ] References `global-json-file: global.json` (not hardcoded version)
- [ ] `rollForward` policy set (recommended: `latestMinor`)
- [ ] `allowPrerelease: false` for stable builds

**Validation:**
```yaml
- name: Setup .NET
  uses: actions/setup-dotnet@v5
  with:
    global-json-file: global.json
```

**global.json:**
```json
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestMinor",
    "allowPrerelease": false
  }
}
```

**Red Flags:**
- ❌ Hardcoded SDK version in workflow (bypasses global.json)
- ❌ `Global.json` (wrong case on Linux runners)
- ❌ No `rollForward` policy (misses security patches)

---

### 3. Build Commands

**Check:**
- [ ] `dotnet restore` before `dotnet build`
- [ ] `--configuration Release` for production-like builds
- [ ] `--no-restore` in build step (restore is separate)
- [ ] Solution file explicitly referenced (not default discovery)
- [ ] `--no-build` in test steps (leverages cached artifacts)

**Validation:**
```bash
dotnet restore
dotnet build MySolution.sln --configuration Release --no-restore
dotnet test tests/MyProject --configuration Release --no-build --no-restore
```

**Red Flags:**
- ❌ Debug configuration in CI (not production-like)
- ❌ Missing `--no-restore` (wastes time re-restoring)
- ❌ Missing `--no-build` (rebuilds in every test job)

---

### 4. Test Execution

**Check:**
- [ ] Test projects exist and are discoverable
- [ ] Directory validation before test execution
- [ ] Explicit exit code checking (`exit_code=$?`)
- [ ] Error messages use `::error::` annotation
- [ ] Timeout protection (10-15 min per test job)

**Validation:**
```bash
if [ ! -d "tests/Unit" ]; then
  echo "::error::Unit test project not found at tests/Unit"
  exit 1
fi

dotnet test tests/Unit --configuration Release --no-build
exit_code=$?
if [ $exit_code -ne 0 ]; then
  echo "::error::Unit tests failed"
fi
exit $exit_code
```

**Red Flags:**
- ❌ No directory validation (silent failures)
- ❌ No exit code checking (tests fail but job succeeds)
- ❌ No timeout (runaway jobs)

---

### 5. Coverage Tool Integration

**Check:**
- [ ] Coverlet.Collector package in Directory.Packages.props
- [ ] `--collect:"XPlat Code Coverage"` in test commands
- [ ] ReportGenerator package: `dotnet-reportgenerator-globaltool` (not old package)
- [ ] Coverage aggregation job waits for all test jobs
- [ ] Threshold warnings (non-blocking, e.g., <80%)
- [ ] Architecture tests excluded if using NetArchTest (incompatibility)

**Validation:**
```yaml
# Test job
- name: Run Unit Tests
  run: |
    dotnet test tests/Unit \
      --configuration Release \
      --no-build \
      --collect:"XPlat Code Coverage" \
      --results-directory test-results

# Coverage job
- name: Install ReportGenerator
  run: dotnet tool install -g dotnet-reportgenerator-globaltool

- name: Generate coverage report
  run: |
    reportgenerator \
      -reports:"coverage-reports/**/**/coverage.cobertura.xml" \
      -targetdir:"./coverage-output" \
      -reporttypes:"Html;Cobertura;JsonSummary"
```

**Red Flags:**
- ❌ Old ReportGenerator package name (tool install fails)
- ❌ Coverage threshold as hard failure (blocks CI on minor drop)
- ❌ NetArchTest tests included in coverage (causes failures)

---

### 6. Job Dependency Graph

**Check:**
- [ ] Build job runs first (caches artifacts)
- [ ] Test jobs depend on build (`needs: build`)
- [ ] Test jobs run in parallel (no unnecessary dependencies)
- [ ] Coverage job waits for all test jobs (`needs: [test-unit, test-integration, ...]`)
- [ ] Report job waits for all tests + coverage
- [ ] Jobs use `if: always()` for reporting even on failures

**Validation:**
```yaml
jobs:
  build:
    runs-on: ubuntu-latest
    # ... build steps

  test-unit:
    needs: build
    # ... unit tests

  test-integration:
    needs: build
    # ... integration tests

  coverage:
    needs: [test-unit, test-integration]
    if: always()
    # ... coverage aggregation

  report:
    needs: [build, test-unit, test-integration]
    if: always()
    # ... test result publishing
```

**Red Flags:**
- ❌ Test jobs don't depend on build (redundant compilation)
- ❌ Test jobs depend on each other (unnecessary serialization)
- ❌ Report job missing `if: always()` (no report on failure)

---

### 7. Caching Strategy

**Check:**
- [ ] NuGet package caching (`${{ github.workspace }}/.nuget/packages`)
- [ ] Build artifact caching (`**/bin/`, `**/obj/`)
- [ ] Cache key includes `.csproj`, `Directory.Packages.props`, `global.json`
- [ ] Restore keys for fallback caching
- [ ] Build job saves cache, test jobs restore cache

**Validation:**
```yaml
# Build job
- name: Cache NuGet packages
  uses: actions/cache@v5
  with:
    path: ${{ github.workspace }}/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj', '**/Directory.Packages.props') }}
    restore-keys: |
      ${{ runner.os }}-nuget-

- name: Cache build artifacts
  uses: actions/cache@v5
  with:
    path: |
      **/bin/Release/
      **/obj/
    key: ${{ runner.os }}-build-${{ hashFiles('**/*.csproj', '**/Directory.Packages.props', 'global.json') }}
    restore-keys: |
      ${{ runner.os }}-build-
```

**Red Flags:**
- ❌ No caching (wastes 10-15 min per run)
- ❌ Cache key missing `global.json` (misses SDK changes)
- ❌ No restore keys (cache miss = hard failure)

---

### 8. Service Containers (Integration Tests)

**Check:**
- [ ] Database service containers defined (MongoDB, SQL Server, etc.)
- [ ] Health checks configured (`--health-cmd`, `--health-interval`)
- [ ] Connection strings in `env:` variables
- [ ] Correct image version (e.g., `mongo:7.0` for MongoDB 7.x)

**Validation:**
```yaml
test-integration:
  services:
    mongodb:
      image: mongo:7.0
      ports:
        - 27017:27017
      options: >-
        --health-cmd mongosh
        --health-interval 10s
        --health-timeout 5s
        --health-retries 5

  env:
    MONGODB_CONNECTION_STRING: "mongodb://localhost:27017/test-db"
```

**Red Flags:**
- ❌ No health checks (tests run before DB is ready)
- ❌ Wrong image version (incompatible with code)
- ❌ Hardcoded connection strings in code (not env vars)

---

## Performance Optimization Patterns

### Build Artifact Caching (Recommended)

**Pattern:**
1. Build job compiles solution once and caches `bin/` and `obj/`
2. Test jobs restore from cache and use `--no-build`
3. Saves 10-15 minutes per run (after first run)

**Trade-off:** First run or cache miss requires manual re-trigger

### Parallel Test Execution (Essential)

**Pattern:**
1. All test jobs run concurrently (no dependencies between them)
2. Each job is idempotent (no shared state)
3. Saves 15+ minutes vs. sequential execution

**Trade-off:** More runner minutes consumed (but wall time reduced)

### Reusable Workflow Pattern (Maintainability)

**Pattern:**
1. Thin orchestrator workflow (versioning, notifications)
2. Reusable test suite workflow (build, test, coverage)
3. Single source of truth, eliminates duplication

**Benefits:**
- 60%+ reduction in orchestrator workflow lines
- Easy to add callers without duplication
- Centralized test execution logic

---

## Common Issues and Solutions

### Issue: Tests fail with "missing binaries" error

**Cause:** Build artifact cache miss, test job uses `--no-build`  
**Solution:** Re-trigger workflow (cache will be populated on next build)  
**Prevention:** Add restore-keys for graceful fallback

### Issue: Coverage reports not generated

**Cause:** Wrong ReportGenerator package name  
**Solution:** Use `dotnet-reportgenerator-globaltool` (not old package)  
**Prevention:** Verify package name in review

### Issue: Architecture tests fail with coverage enabled

**Cause:** NetArchTest incompatibility with Coverlet  
**Solution:** Exclude architecture tests from coverage collection  
**Prevention:** Document in coverage job (expected behavior)

### Issue: SDK version mismatch between local and CI

**Cause:** Hardcoded SDK version in workflow, bypasses global.json  
**Solution:** Use `actions/setup-dotnet@v5` with `global-json-file: global.json`  
**Prevention:** Always reference global.json in SDK setup

### Issue: Case-sensitive file errors on Linux runners

**Cause:** Windows case-insensitive, Linux case-sensitive  
**Solution:** Use lowercase filenames (`global.json` not `Global.json`)  
**Prevention:** Test workflows on Linux runners before merge

---

## Review Output Template

Use this template when documenting review findings:

```markdown
# {Workflow Name} Review — Issue #{Number}

**Reviewer:** {Your Name}  
**Date:** {YYYY-MM-DD}  
**Issue:** #{Number} — {Title}  
**Branch:** {branch-name}  

## Executive Summary

{Overall assessment: EXCELLENT / GOOD / NEEDS FIXES}

{Brief summary of key findings}

## Detailed Findings

### 1. ✅/❌ Workflow Triggers
{Analysis}

### 2. ✅/❌ .NET SDK Setup
{Analysis}

### 3. ✅/❌ Build Commands
{Analysis}

### 4. ✅/❌ Test Execution
{Analysis}

### 5. ✅/❌ Coverage Integration
{Analysis}

### 6. ✅/❌ Job Dependencies
{Analysis}

### 7. ✅/❌ Caching Strategy
{Analysis}

### 8. ✅/❌ Service Containers
{Analysis}

## Performance Characteristics

- Build job: {X} minutes
- Test jobs: {X} minutes each
- Total: {X} minutes

## Recommendations

{Immediate actions / Future enhancements}

## Conclusion

{Overall verdict and recommendation}
```

---

## Related Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [.NET SDK Global JSON](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json)
- [Coverlet Documentation](https://github.com/coverlet-coverage/coverlet)
- [ReportGenerator Documentation](https://github.com/danielpalme/ReportGenerator)

---

## Skill Maintenance

**Last Updated:** 2026-02-20  
**Update Frequency:** Quarterly or after major .NET SDK releases  
**Owner:** Legolas (DevOps/Infrastructure)  
