# Decision: Remove E2E Test Infrastructure

**Date:** 2026-02-19  
**Author:** Gandalf (Lead Architect)  
**Status:** Implemented  

---

## Context

IssueManager is an Aspire-orchestrated .NET 10 application with multiple test layers:
- Unit tests (business logic, validators, handlers)
- Architecture tests (layer boundaries, naming conventions)
- Integration tests (MongoDB TestContainers, end-to-end data flows)
- Blazor tests (bUnit component testing)
- Aspire tests (orchestration health)
- E2E tests (Playwright browser automation)

**Problem:** The E2E test infrastructure requires a running web application endpoint to execute Playwright browser automation tests. However:
- Aspire project is in early development stage
- Web application endpoints are not yet stable or externally accessible
- E2E tests cannot execute without a deployed web URL
- CI/CD pipeline fails when attempting to run E2E tests (missing browser dependencies, no endpoint to test against)

---

## Decision

**Remove the E2E test infrastructure entirely:**
1. Delete `tests/E2E/` directory and all E2E test code
2. Remove E2E project reference from `IssueManager.sln`
3. Update CI/CD workflow to remove `test-e2e` job (delegated to Legolas/DevOps)
4. Update test documentation to reflect adjusted testing strategy (delegated to Gimli/Tester)

**Rationale:**
- E2E tests provide value only when web application is deployable and accessible
- Maintaining E2E infrastructure without ability to execute tests creates false confidence
- Other test layers (Unit, Integration, Architecture, Blazor) provide adequate coverage during early development
- Faster CI/CD execution without longest-running test job (E2E typically 3-5 minutes)
- Reduced maintenance burden (no Playwright browser version management)

---

## Alternatives Considered

### 1. Keep E2E tests but disable them
**Pros:** Infrastructure ready when web app is deployable  
**Cons:** Dead code creates maintenance burden, CI/CD confusion, false impression of coverage  
**Rejected:** Violates "don't maintain what you can't execute" principle

### 2. Mock or stub web application endpoint
**Pros:** E2E tests can run without real web app  
**Cons:** Mocked E2E tests are not true end-to-end tests, defeats purpose of browser automation  
**Rejected:** Playwright tests without real web app provide no value over bUnit component tests

### 3. Wait for Aspire endpoints to stabilize
**Pros:** Keep all test infrastructure in place  
**Cons:** Timeline unclear, CI/CD fails in meantime, blocks other development  
**Rejected:** Premature optimization, revisit when web app is deployable

---

## Implementation

**Completed Actions:**
- ✅ Deleted `tests/E2E/` directory (entire project)
- ✅ Removed E2E project reference from solution using `dotnet sln remove tests\E2E\E2E.csproj`
- ✅ Verified solution builds successfully: `dotnet build IssueManager.sln --configuration Release` (exit code 0)
- ✅ Updated Gandalf history (`history.md`) with architectural decision and context

**Pending Actions (delegated):**
- ⏳ Legolas (DevOps): Remove `test-e2e` job from `.github/workflows/test.yml`
- ⏳ Gimli (Tester): Update test documentation to remove E2E references and adjust coverage strategy

---

## Consequences

### Positive
- **Faster CI/CD:** Removed longest-running test job (3-5 minutes saved per run)
- **Reduced complexity:** No Playwright browser management, no endpoint configuration
- **Honest coverage:** Test suite reflects actual executable tests, no false positives
- **Clear signal:** When web app is deployable, E2E infrastructure will be re-introduced with purpose

### Negative
- **Lost coverage:** No end-to-end user workflow validation via browser automation
- **Manual testing required:** Critical paths must be manually tested until E2E tests are restored
- **Re-work later:** Will need to recreate E2E infrastructure when Aspire endpoints are stable

### Neutral
- **No impact on other test layers:** Unit, Integration, Architecture, Blazor tests remain intact
- **No blocking issues:** Solution builds, other tests run successfully

---

## Monitoring & Success Criteria

**How we'll know this was the right decision:**
1. CI/CD pipeline passes consistently without E2E test failures
2. Development velocity increases (no debugging E2E infrastructure issues)
3. Other test layers provide sufficient confidence for early development

**When to revisit:**
- Aspire web application is deployed to a stable endpoint (dev/staging environment)
- Web UI is mature enough for user workflow testing
- Team requests E2E coverage for critical paths

**Re-introduction criteria:**
- Web app accessible via stable URL (local, dev, or staging)
- Playwright browser automation can successfully navigate application
- E2E tests provide value beyond existing bUnit component tests

---

## Related Work Items

- **Legolas (DevOps):** Update `.github/workflows/test.yml` to remove `test-e2e` job
- **Gimli (Tester):** Update test documentation (`docs/testing/`) to reflect adjusted strategy
- **Future (TBD):** Re-introduce E2E tests when Aspire endpoints are stable

---

## References

- [Playwright Documentation](https://playwright.dev/dotnet/)
- [Aspire Orchestration Docs](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Test Pyramid Pattern](https://martinfowler.com/articles/practical-test-pyramid.html)

---

**Sign-off:** Gandalf (Lead Architect)  
**Review:** Pending (Legolas for CI/CD impact, Gimli for test strategy impact)
