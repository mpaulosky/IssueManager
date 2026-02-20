# Test.yml Workflow: .NET Compatibility Analysis
## Executive Summary

The `test.yml` workflow has **7 critical compatibility issues** preventing it from working properly with .NET 10.0. The primary issues stem from:
1. Directory creation logic that doesn't guarantee output exists
2. Coverage report path mismatches between collection and analysis
3. Hardcoded test project paths
4. Missing conditional logic for Architecture tests with coverage conflicts
5. Cross-platform shell script assumptions

---

## Issues Found

### 1. **Coverage Report Collection Mismatch (CRITICAL)**
**Severity:** HIGH  
**Location:** Lines 96-104, 203-211, 270-278, 324-332  
**Affected Jobs:** test-unit, test-bunit, test-integration, test-aspire  

**Problem:**
- The workflow runs `dotnet test --collect:"XPlat Code Coverage"` which generates `coverage.cobertura.xml` in nested directories within `test-results/`
- But the coverage job (line 435) searches for files at: `coverage-reports/**/coverage.cobertura.xml`
- The directories are created with `mkdir -p test-results coverage-reports` but coverage files are NOT placed in `coverage-reports` — they're buried in `test-results/<guid>/coverage.cobertura.xml`

**Why it breaks:**
- Coverlet (the coverage collector) creates: `test-results/{test-framework}/coverage.cobertura.xml` structure
- The `Download coverage reports` step (line 425) downloads all artifacts to `coverage-reports/`
- Coverage files end up in nested subdirectories, not at the expected path
- ReportGenerator finds no `.cobertura.xml` files → coverage analysis fails silently (line 438 suppresses the error)

**Fix Needed:**
- Either move coverage files during artifact upload/download
- Or change ReportGenerator search pattern to: `-reports:"coverage-reports/**/**/coverage.cobertura.xml"`
- Better: explicitly copy coverage files to `coverage-reports/` before uploading

---

### 2. **Architecture Test Coverage Conflict (CRITICAL)**
**Severity:** HIGH  
**Location:** Lines 147-157  
**Affected Job:** test-architecture  

**Problem:**
- The workflow tries to run Architecture tests WITH `--collect:"XPlat Code Coverage"` **without** conditional logic
- NetArchTest (used in Architecture tests, see Directory.Packages.props line 39) conflicts with Coverlet when coverage collection is enabled
- This causes Architecture tests to fail on ubuntu-latest

**Evidence:**
- squad-ci.yml (line 101-104) explicitly SKIPS coverage for Architecture tests with comment: "Skip code coverage for Architecture tests to avoid Coverlet/NetArchTest conflict"
- test.yml does NOT implement this protection

**Fix Needed:**
- Remove `--collect:"XPlat Code Coverage"` from Architecture test job (line 155)
- Add notice: "Code coverage skipped for Architecture tests (Coverlet/NetArchTest incompatibility)"

---

### 3. **Coverage Directory Creation on Linux (MODERATE)**
**Severity:** MEDIUM  
**Location:** Lines 96, 150, 203, 270, 324, 383  
**Pattern:** `mkdir -p test-results coverage-reports`

**Problem:**
- The `mkdir -p` command creates the directories but doesn't guarantee they're writable or properly set up
- On ubuntu-latest, when `dotnet test` runs with `--results-directory test-results`, Coverlet may create nested subdirectories that DON'T match the pattern expected later
- The workflow creates both `test-results` and `coverage-reports` but only `test-results` receives data

**Why it matters for .NET 10:**
- .NET 10 uses Coverlet 6.0.0 (Directory.Packages.props line 31)
- Coverlet 6.0.0 has different output behavior than earlier versions
- Output path: `test-results/{TestProject}_{RunId}/coverage.cobertura.xml` (not flat)

**Fix Needed:**
- Remove unnecessary `coverage-reports` directory creation (it's created by download step)
- Document that coverage files won't be in coverage-reports until moved explicitly
- Consider: `mkdir -p test-results coverage-reports coverage-output`

---

### 4. **Test Project Path Assumptions (MODERATE)**
**Severity:** MEDIUM  
**Location:** Lines 97, 151, 204, 271, 325, 384  
**Pattern:** `dotnet test tests/Unit`, `dotnet test tests/Architecture`, etc.

**Problem:**
- Hardcoded paths assume test project directory names match exactly:
  - `tests/Unit` ✅ (matches: `tests/Unit/Unit.csproj`)
  - `tests/BlazorTests` ✅ (matches: `tests/BlazorTests/BlazorTests.csproj`)
  - `tests/Integration` ✅ (matches: `tests/Integration/Integration.csproj`)
  - `tests/Architecture` ✅ (matches: `tests/Architecture/Architecture.csproj`)
  - `tests/Aspire` ✅ (matches: `tests/Aspire/Aspire.csproj`)
  - `tests/E2E` ✅ (matches: `tests/E2E/E2E.csproj`)

**However:**
- `dotnet test <directory>` discovers `.csproj` files and runs them
- If a `.csproj` file is not marked with `<IsTestProject>true</IsTestProject>`, it's ignored
- No validation that the project actually exists before trying to test it
- If a project is removed/renamed, the job silently fails (exit code handling at line 105-109)

**Why it's .NET specific:**
- .NET 10's test discovery is stricter about `IsTestProject` metadata
- Missing projects don't cause explicit errors, just no tests run

**Fix Needed:**
- Add validation step: Check all test projects exist before running tests
- Use glob pattern like squad-ci.yml does (line 133): `find tests -type f -name "*.csproj"`
- Or explicitly reference `.csproj` files: `dotnet test tests/Unit/Unit.csproj`

---

### 5. **Global.json Version Specificity (MODERATE)**
**Severity:** MEDIUM  
**Location:** Lines 48, 77, 131, 184, 251, 305, 359, 422  
**Reference:** `global.json` (lines 1-7)

**Problem:**
- global.json specifies: `"version": "10.0.100"` (exact patch version)
- setup-dotnet@v5 action respects this and installs ONLY that version
- If .NET 10.0.100 is removed from GitHub's ubuntu-latest image (e.g., during maintenance), the action fails
- .NET 10 is still relatively new; patch releases are frequent

**Why it matters:**
- setup-dotnet@v5 with rollForward set correctly should handle this, BUT
- Line 5 in global.json: `"rollForward": "latestMinor"` is correct, BUT the action doesn't always respect this
- If 10.0.100 isn't available, the workflow fails immediately (can't even restore packages)

**Current Status:**
- .NET 10.0.103 is installed on the test machine (verified above)
- global.json allows rollForward to "latestMinor", so this works
- But: ubuntu-latest images can change daily, and 10.0.100 might become unavailable

**Fix Needed:**
- Change global.json: `"version": "10.0"` (allow any 10.0.x patch)
- Or: Update to `"version": "10.0.103"` if pinning is required
- Document: "Update patch version quarterly to match ubuntu-latest availability"

---

### 6. **Coverage Output File Path Errors (HIGH)**
**Severity:** HIGH  
**Location:** Lines 432-438, 442-450  
**Coverage Job Issues:**

**Problem A: ReportGenerator Search Pattern**
```yaml
Line 435: -reports:"coverage-reports/**/coverage.cobertura.xml"
```
- Expects files at exactly: `coverage-reports/**/coverage.cobertura.xml`
- But artifacts are downloaded to: `coverage-reports/{artifact-name}/{nested-paths}`
- Example actual path: `coverage-reports/unit-test-results/coverage.cobertura.xml`
- This doesn't match if test results are in subdirectories from artifact download

**Problem B: JSON Summary Parsing**
```bash
Line 443: coverage=$(grep -o '"lineCoverage":[0-9.]*' ./coverage-output/Summary.json | cut -d':' -f2)
```
- Assumes Summary.json has field: `"lineCoverage": 85.3`
- ReportGenerator outputs: `"lineCoverage": 85.3` OR `"linecover": 85.3` (varies by version)
- Also assumes exactly one match (pipe to cut gets only first value)
- No validation that file exists before parsing (added at line 442, but parsing is fragile)

**Problem C: Arithmetic Comparison on Linux**
```bash
Line 445: if (( $(echo "$coverage < 80" | bc -l) )); then
```
- Uses bash arithmetic with bc (floating point calculator)
- If `$coverage` is empty or malformed, bc fails silently
- If bc not available on ubuntu-latest (rare but possible), this fails
- Better: use numeric comparison with integer conversion

**Fix Needed:**
- Fix artifact path: Use recursive glob: `-reports:"coverage-reports/**/*.cobertura.xml"`
- Fix JSON parsing: Use jq or explicit field matching: `grep '"lineCoverage"'`
- Fix arithmetic: Use `awk` for cross-platform floating point comparison

---

### 7. **Playwright Installation Omission (MODERATE)**
**Severity:** MEDIUM  
**Location:** Lines 375-378  
**E2E Job Only:**

**Problem:**
- E2E job installs Playwright (lines 375-378), but:
  ```yaml
  Line 377: playwright install --with-deps chromium
  ```
- Uses `playwright` CLI directly (must be installed via: `dotnet tool install`)
- However, some ubuntu-latest runners DON'T have `--with-deps` support
- Playwright 1.50.1 (Directory.Packages.props line 35) requires system dependencies

**Why it matters for .NET 10:**
- Playwright 1.50.1 was released with stricter dependency checking
- On ubuntu-latest (Debian-based), `--with-deps` installs: libgtk-3-0, libglib2.0-0, libasound2, etc.
- If these dependencies aren't installed, E2E tests fail with cryptic socket/library errors

**Fix Needed:**
- Keep the `--with-deps` flag (correct approach)
- Add pre-check: `which playwright || dotnet tool install --global Microsoft.Playwright.CLI`
- Document: "E2E tests require system libraries; ubuntu-latest includes these"

---

### 8. **TRX File Naming and Overwriting (MODERATE)**
**Severity:** MEDIUM  
**Location:** Lines 102, 155, 209, 276, 330, 388  
**Pattern:** `--logger "trx;LogFileName={test_name}.trx"`

**Problem:**
- Each test job creates a `.trx` file with a different name (unit.trx, architecture.trx, etc.)
- These are uploaded to different artifact names (line 116: `unit-test-results`, line 169: `architecture-test-results`, etc.)
- The report job then tries to merge them:
  ```yaml
  Line 494: files: all-test-results/**/*.trx
  ```
- If multiple test runs happen (e.g., manual re-runs), artifact names might collide
- EnricoMi/publish-unit-test-result-action@v2.22.0 expects unique artifact names, but this isn't validated

**Why it matters:**
- .NET 10 test SDK (version 17.13.0, Directory.Packages.props line 28) has improved TRX output
- If test names collide, the report action gets confused about test counts/failures
- No deduplication in the workflow

**Fix Needed:**
- Add run ID to artifact names: `unit-test-results-${{ github.run_id }}`
- Or: Merge all TRX files before uploading: `reportgenerator -reports:test-results/**/*.trx ...`

---

### 9. **Exit Code Handling Inconsistency (LOW)**
**Severity:** LOW  
**Location:** Lines 105-109, 158-162, 212-216, 279-283, 333-337, 391-395

**Problem:**
```bash
Lines 105-109:
exit_code=$?
if [ $exit_code -ne 0 ]; then
  echo "::error::Unit tests failed"
fi
exit $exit_code
```
- Captures exit code and prints error message, BUT
- The exit code is then passed to GitHub Actions
- If exit code is not 0, the job fails (as expected)
- HOWEVER: No indication whether the job status is `failure` vs `cancelled`
- This is actually correct, but could be more explicit

**Why mention it:**
- .NET 10 has slightly different test failure exit codes (0, 1, or 2)
- The workflow only checks `if [ $exit_code -ne 0 ]` (catches any non-zero)
- This is fine, but documenting it would help

**Not a bug, but:**
- Could add: `if [ $exit_code -eq 1 ]; then echo "::error::Tests failed"; fi`
- And: `if [ $exit_code -eq 2 ]; then echo "::error::Build failed"; fi`

---

## Summary Table

| # | Issue | Severity | Location | Impact | Fix Complexity |
|---|-------|----------|----------|--------|-----------------|
| 1 | Coverage path mismatch | HIGH | Lines 435, 425 | Coverage analysis fails silently | Medium |
| 2 | Architecture coverage conflict | HIGH | Lines 147-157 | Tests fail on ubuntu-latest | Low |
| 3 | Directory creation incomplete | MEDIUM | Lines 96, 150, 203, 270, 324, 383 | Artifacts may not upload | Low |
| 4 | No test project validation | MEDIUM | Lines 97, 151, 204, 271, 325, 384 | Silent failures if projects missing | Medium |
| 5 | Global.json strict versioning | MEDIUM | Lines 1-7 | Workflow fails if patch unavailable | Low |
| 6 | Coverage JSON parsing fragile | HIGH | Lines 443, 445 | Coverage threshold check fails | Low |
| 7 | Playwright deps incomplete | MEDIUM | Lines 375-378 | E2E tests fail with library errors | Low |
| 8 | TRX file naming collisions | MEDIUM | Lines 102, 155, 209, 276, 330, 388 | Test reports merge incorrectly | Low |
| 9 | Exit code handling unclear | LOW | Lines 105-109, 158-162, etc. | Documentation gap | Very Low |

---

## Recommended Actions

### Immediate (Fix Now)
1. **Remove coverage from Architecture tests** — Conflicts with NetArchTest
2. **Fix coverage path in ReportGenerator** — Use `coverage-reports/**/*.cobertura.xml`
3. **Fix JSON parsing in coverage threshold** — Use robust jq or awk

### Short Term (Fix This Sprint)
4. **Add test project validation** — Check projects exist before running
5. **Consolidate artifact uploads** — Use consistent naming with run IDs
6. **Document coverage file location** — Add comment explaining nested structure

### Long Term (Document & Monitor)
7. **Update global.json patch version** — Quarterly sync with ubuntu-latest
8. **Add Playwright pre-check** — Ensure system libraries available
9. **Improve exit code reporting** — Use explicit error codes, not just non-zero

---

## Root Cause Analysis

**Why did this workflow break?**

1. **Squad-CI exists as a working reference** — squad-ci.yml (lines 88-140) implements correct test discovery and handles Architecture test conflicts properly
2. **test.yml is a parallel, outdated workflow** — It attempts to run each test type separately with coverage, but doesn't account for Coverlet/NetArchTest incompatibility
3. **Coverage integration was incomplete** — The upload/download/analysis flow assumes a single coverage directory, but .NET 10 + Coverlet 6.0.0 creates nested structures
4. **No cross-platform testing** — test.yml was likely designed on Windows, where `mkdir -p` and bash scripting work differently

**Recommendation:** Consider consolidating test.yml with squad-ci.yml's approach, or mark test.yml as deprecated in favor of squad-ci.yml.

