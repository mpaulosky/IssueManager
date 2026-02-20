# Decision: Remove E2E Test Jobs from GitHub Actions Workflows

**Date:** 2026-02-17  
**Author:** Legolas (DevOps/Infrastructure)  
**Status:** Implemented  
**Impact:** CI/CD Pipeline

---

## Context

The IssueManager project is an **Aspire-based application** where:
- Service discovery is dynamic (ephemeral ports, service mesh routing)
- The Blazor web UI does not expose a static, predictable endpoint suitable for E2E testing
- Playwright E2E tests cannot reliably target the application without complex orchestration

The existing GitHub Actions workflow included a `test-e2e` job that:
- Installed Playwright browsers (~2-3 minutes)
- Attempted to run E2E tests against a non-existent endpoint (~3-5 minutes)
- Always failed or was skipped, adding noise to CI results

---

## Decision

**Remove all E2E test job definitions** from GitHub Actions workflows:

1. **From `.github/workflows/test.yml`:**
   - Removed the `test-e2e` job entirely (lines 366-431)
   - Updated `report` job's `needs` list to exclude `test-e2e`
   - Removed `e2e_status` variable from job summary script
   - Removed "E2E Tests" line from status report

2. **From `.github/workflows/squad-ci.yml`:**
   - Verified no E2E references exist (already clean)

3. **Validation:**
   - YAML syntax validated — both workflows parse correctly
   - Job dependency graph verified — no dangling references

---

## Rationale

### Why This Decision Was Made

1. **Aspire architecture incompatibility:**
   - Aspire services use dynamic port allocation
   - No static web endpoint available for Playwright tests
   - E2E tests require pre-deployed, accessible web servers

2. **CI pipeline efficiency:**
   - Removes ~5 minutes from pipeline (Playwright install + test execution)
   - Reduces parallel job count from 6 to 5 (cost savings)
   - Eliminates failing/skipped job noise in CI reports

3. **Test coverage preserved:**
   - Unit tests verify business logic
   - bUnit tests cover Blazor component behavior
   - Integration tests validate MongoDB interactions
   - Aspire tests ensure orchestration works
   - **Architecture tests enforce structural rules**

### Alternatives Considered

| Alternative | Pros | Cons | Decision |
|-------------|------|------|----------|
| **Deploy Aspire to temp K8s/container in CI** | Enables real E2E testing | Adds 10-15 min to pipeline; complex orchestration; cost increase | Rejected (not worth trade-off) |
| **Mock the web endpoint with test server** | Faster than deployment | Not true E2E; doesn't test real service mesh | Rejected (integration tests already cover this) |
| **Run E2E tests manually in staging** | Keeps CI fast | No automated regression detection | Accepted (future option if needed) |
| **Remove E2E jobs entirely** | Clean CI pipeline; no wasted time | Loses one layer of test coverage | **Accepted** (other test types compensate) |

---

## Consequences

### Positive
- **Faster CI pipeline:** ~10-12 minutes vs previous 12-15 minutes
- **Cleaner job reports:** No failing/skipped E2E job cluttering results
- **Reduced resource usage:** 5 parallel jobs instead of 6 (cost savings)
- **Clearer test intent:** Test suite now focuses on in-process/integration testing

### Negative
- **No browser-based E2E coverage:** Cannot detect UI regressions via automation
- **Risk of UI bugs:** Blazor component tests (bUnit) may not catch all rendering issues
- **Manual testing burden:** Developers must manually test UI flows in local/staging environments

### Mitigation
- **bUnit coverage increase:** Expand Blazor component tests to cover critical user flows
- **Manual testing checklist:** Create pre-deployment UI smoke test checklist
- **Staging environment:** Deploy to staging for manual validation before production
- **Future E2E strategy:** If needed, implement post-deployment E2E tests against staging environment

---

## Implementation

### Files Modified
1. **`.github/workflows/test.yml`** (4 edits):
   - Removed `test-e2e` job definition (65 lines)
   - Updated `report` job's `needs` list (removed `test-e2e`)
   - Updated job summary script (removed `e2e_status` variable)
   - Updated summary output (removed "E2E Tests" line)

2. **`.github/workflows/squad-ci.yml`**:
   - No changes required (already clean)

### Remaining Jobs in `test.yml`
1. `build` — Solution build with NuGet caching
2. `test-unit` — Unit tests with coverage
3. `test-architecture` — NetArchTest architectural rules
4. `test-bunit` — Blazor component tests with coverage
5. `test-integration` — Integration tests with MongoDB
6. `test-aspire` — Aspire orchestration tests with coverage
7. `coverage` — Aggregate coverage analysis
8. `report` — Unified test result summary

---

## Validation

✅ **YAML syntax validated** — both workflows parse correctly  
✅ **Job dependency graph verified** — no dangling references to `test-e2e`  
✅ **No E2E references remain** — grep confirmed clean removal  
✅ **Workflow still functional** — `report` job now depends on 5 test jobs instead of 6  

---

## Follow-Up Actions

- [ ] **Gandalf:** Delete `tests/E2E/` directory and remove E2E project from solution
- [ ] **Gimli:** Update `README.md` to reflect current test strategy (no E2E tests)
- [ ] **Team:** Consider future E2E strategy if UI regression detection becomes critical

---

## References

- **Task:** Remove E2E Test Jobs from Workflows
- **Charter:** `.ai-team/agents/legolas/charter.md`
- **History:** `.ai-team/agents/legolas/history.md` (updated with E2E removal learnings)
- **Related:** `.github/workflows/test.yml`, `.github/workflows/squad-ci.yml`
