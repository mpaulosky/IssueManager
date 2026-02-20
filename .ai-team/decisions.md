# Decisions — IssueManager Squad

*This is the authoritative ledger of team decisions. All decisions are append-only.*

## 2026-02-17: Team formation and initial architecture

**By:** Gandalf (via Coordinator)  
**What:** Squad team established with 5 agents: Gandalf (Lead), Aragorn (Backend), Arwen (Frontend), Gimli (Tester), Legolas (DevOps)  
**Why:** IssueManager is a complex CQRS + vertical slice application requiring architectural oversight, backend expertise, frontend work, quality assurance, and infrastructure coordination

**Details:**
- Universe: The Lord of the Rings
- Tech stack: C#, .NET 10.0, Aspire, Blazor, MongoDB.EntityFramework, CQRS
- Architecture: Vertical Slice Architecture with CQRS pattern
- Scribe and Ralph are included (memory + work monitoring)

---

## 2026-02-19: PR #14 Review — Playwright CI Integration (REJECTED)

**By:** Gandalf (Lead)  
**What:** Reviewed PR #14 attempting to fix E2E test failures by installing Playwright browsers in CI  
**Result:** NEEDS FIXES — Critical issue found: wrong workflow file modified

**Analysis:**
- **Root cause identified correctly:** E2E tests failing due to missing Chromium browser binaries in GitHub Actions
- **Solution approach is sound:** Install Playwright CLI tool and Chromium browsers before running tests
- **CRITICAL FLAW:** PR modifies `squad-ci.yml` but E2E tests actually run in `test.yml` workflow (test-e2e job)
- **squad-ci.yml** is a single-job workflow for simple build+test verification (line 30: "build-and-test" job)
- **test.yml** is the comprehensive test suite with 6 parallel jobs including test-e2e (lines 346-398)
- The Playwright install step must be added to test.yml (test-e2e job) not squad-ci.yml

**Minor issues:**
- Uses `dotnet tool update --global ... || dotnet tool install --global ... --ignore-failed-sources` which is overly complex
- Should use simple `dotnet tool install --global Microsoft.Playwright.CLI` for consistency with test.yml conventions
- Uses `--with-deps` flag which is actually better than separate install/install-deps commands (improvement)

**Impact if merged:**
- E2E tests in test.yml will continue to fail (no Playwright browsers installed)
- squad-ci.yml will have an unnecessary Playwright install step (no E2E tests run there)
- Test failures will persist, blocking future PRs

**Decision:**
NEEDS FIXES before merge. Legolas (DevOps) should:
1. Remove Playwright install from squad-ci.yml
2. Add Playwright install to test.yml in test-e2e job after "Build solution" step (line 373)
3. Use command: `dotnet tool install --global Microsoft.Playwright.CLI && playwright install --with-deps chromium`
4. Re-run tests to verify E2E tests now pass

---

*End of decisions. Append new decisions below as the team works.*
