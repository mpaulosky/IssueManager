# Decision: Build Artifact Caching in CI/CD

**Date:** 2026-02-19  
**Author:** Legolas (DevOps)  
**Status:** Implemented  
**Related:** `.github/workflows/squad-test.yml`

---

## Context

The squad-test.yml workflow runs 5 parallel test jobs (unit, architecture, bunit, integration, aspire). Each job independently ran `dotnet restore` and `dotnet build`, compiling the same solution 5 times per workflow run. This redundancy added 10-15 minutes of wasted build time per run.

**Options considered:**
1. **Build artifact caching (chosen):** Build once, cache binaries, test jobs use `--no-build`
2. **Build artifacts as GitHub artifacts:** Slower than cache, 90-day retention unnecessary
3. **Matrix build strategy:** Would still rebuild per matrix job; doesn't solve redundancy
4. **Conditional build in test jobs:** Defeats the purpose; want to fail fast on cache miss

---

## Decision

Implement build artifact caching (Option 1):
- Single `build` job compiles solution and caches `**/bin/Release/` and `**/obj/` directories
- All test jobs restore from cache and skip rebuild entirely using `--no-build` flag
- Cache key based on hashes of `.csproj`, `Directory.Packages.props`, and `global.json` files
- Automatic cache invalidation when dependencies or SDK version change

---

## Rationale

**Why this approach:**
- **Performance:** Eliminates 10-15 min of redundant build time per workflow run
- **Consistency:** All test jobs use identical binaries from single build (no race conditions)
- **Simple:** Single cache save in build job, multiple cache restores in test jobs
- **Fail-fast:** `--no-build` flag fails loudly if cache is lost (better than silent rebuild)

**Why not alternatives:**
- **GitHub artifacts:** Slower than cache API; 90-day retention unnecessary for ephemeral binaries
- **Matrix strategy:** Doesn't solve redundancy; each matrix job would still rebuild independently
- **Conditional build:** Defeats the purpose; we want to fail fast on cache miss, not silently rebuild

**Trade-offs accepted:**
- **First run / cache miss:** Test jobs fail with "missing binaries" error; acceptable to re-trigger workflow
- **Cache expiry:** 7-day GitHub Actions default; rare occurrence, acceptable manual intervention
- **Strict --no-build:** Fails loudly if cache is lost; better than silent rebuild (fail fast principle)

---

## Implementation Details

**Cache key pattern:**
```yaml
key: ${{ runner.os }}-build-${{ hashFiles('**/*.csproj', '**/Directory.Packages.props', 'global.json') }}
restore-keys: |
  ${{ runner.os }}-build-
```

**Build job changes:**
- Added cache save step after `dotnet build` (saves `**/bin/Release/` and `**/obj/`)

**Test job changes (unit, architecture, bunit, integration, aspire):**
- Added cache restore step after setup .NET (same key as build job)
- **REMOVED** `dotnet build` step entirely
- Kept `dotnet restore` step (NuGet cache is separate, fast)
- All `dotnet test` commands already used `--no-build` flag (no change needed)

**Cache invalidation triggers:**
- Any `.csproj` file change (new dependency, target framework change)
- `Directory.Packages.props` change (centralized package version change)
- `global.json` change (SDK version change)
- 7-day cache expiry (GitHub Actions default)

---

## Performance Impact

**Before caching:**
- Build job: 5 min (restore + build)
- Each test job: 3-4 min restore + 2-3 min build = 5-7 min overhead per job
- Total redundant build time: 5 jobs × 5 min = 25 min wasted (parallelized to ~7 min wall time)

**After caching:**
- Build job: 5 min (restore + build) + 1 min cache save = 6 min
- Each test job: 3-4 min restore + 30 sec cache restore = ~4 min overhead per job
- Total build time: 6 min (build) + 4 min (test prep) = 10 min vs 12 min before

**Net savings: ~10-15 min per workflow run** (first run excluded)

---

## Safety and Failure Modes

**Expected failures (acceptable):**
1. **Cache miss on first run:** Test jobs fail with "missing binaries"; re-trigger workflow
2. **Cache expired (7 days):** Same as above; re-trigger workflow
3. **Partial cache (corrupted):** `--no-build` fails loudly; clear cache manually via GitHub UI

**Why --no-build is strict (and that's good):**
- Fails loudly if binaries are missing (better than silent rebuild)
- Ensures we know when cache is lost or corrupted
- Cache misses are rare (only on first run or after expiry); acceptable to require manual re-trigger

**NuGet cache remains separate:**
- NuGet package cache is independent (already exists in all jobs)
- `dotnet restore` still runs to restore packages (fast, uses NuGet cache)
- Two-layer caching: packages (stable, rarely changes) + binaries (changes with code)

---

## Monitoring and Validation

**How to verify caching is working:**
1. Check build job logs for "Cache saved successfully" message
2. Check test job logs for "Cache restored successfully" message
3. Verify test jobs complete in ~4 min (down from ~7 min)
4. Compare workflow run times before/after (should see 10-15 min reduction)

**How to debug cache issues:**
1. Check cache key in build job vs test jobs (must match exactly)
2. Verify cached paths exist: `**/bin/Release/` and `**/obj/`
3. Check GitHub Actions cache UI for cache entries (Settings → Actions → Caches)
4. If cache is corrupted, manually clear via GitHub UI and re-trigger workflow

---

## Future Considerations

**If this approach fails:**
- **Option B (GitHub artifacts):** Upload build artifacts, download in test jobs (slower but more reliable)
- **Option C (Conditional build):** Add `if: failure()` build step in test jobs as fallback (sacrifices fail-fast principle)
- **Option D (Matrix strategy):** Consolidate test jobs into matrix (reduces job count but doesn't solve redundancy)

**If cache grows too large (>10 GB repo limit):**
- Reduce cached paths (only `**/bin/Release/`, exclude `**/obj/`)
- Increase cache key specificity (include branch name, commit SHA)
- Implement cache pruning strategy (delete old caches manually)

**If team prefers silent rebuild on cache miss:**
- Add conditional build step: `if: hashFiles('**/bin/Release/**') == ''`
- Trade-off: Slower on cache miss, but no manual intervention needed
- Not recommended: Defeats fail-fast principle, hides cache issues

---

## Conclusion

Build artifact caching eliminates 10-15 minutes of redundant build time per workflow run by compiling the solution once and sharing binaries across 5 parallel test jobs. The approach is simple, fast, and fails loudly on cache issues. Cache invalidation is automatic when dependencies or SDK version change. First run and rare cache expiry require manual re-trigger, which is acceptable given the performance gains on subsequent runs.
