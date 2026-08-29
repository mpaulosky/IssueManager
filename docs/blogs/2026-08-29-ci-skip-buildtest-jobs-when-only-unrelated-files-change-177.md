---
title: "ci: skip build/test jobs when only unrelated files change (#177)"
date: 2026-08-29
commit: fb300bb
author: mpaulosky
---

# ci: skip build/test jobs when only unrelated files change (#177)

## Summary
- The `pull_request` trigger can't use `paths-ignore`/`paths` since
"Build Solution" is a required status check on main's ruleset (see PR
#26) — filtering the trigger would leave that check permanently missing
on docs/config-only PRs, blocking them from merging.
- Added a `changes` job that detects whether `src/**`, `tests/**`,
project/build files, or the workflow itself actually changed (always
true on `workflow_dispatch`/`workflow_call`).
- `build`, and (via cascade or explicit `if:`) `discover-tests`, `test`,
`coverage`, and `report` are now skipped when nothing relevant changed.
GitHub treats a skipped required check as passing, so those PRs still
merge without paying for a full build+test run.

## Test plan
- [x] YAML lint on `ci.yml`
- [x] `dotnet build IssueManager.slnx` — 0 errors
- [x] Pre-push gate (lint + full test suite) passed
- [ ] Confirm on this PR itself that `build`/`test`/`coverage`/`report`
run (since this PR changes `ci.yml`, which is in the `code` path filter)

🤖 Generated with [Claude Code](https://claude.com/claude-code)

## Summary

ci: skip build/test jobs when only unrelated files change (#177)

## Files Changed

```
 .github/workflows/ci.yml | 63 +++++++++++++++++++++++++++++++++++++++++++-----
 1 file changed, 57 insertions(+), 6 deletions(-)
```

## Diff

```diff
diff --git a/.github/workflows/ci.yml b/.github/workflows/ci.yml
index 18a367e..e482702 100644
--- a/.github/workflows/ci.yml
+++ b/.github/workflows/ci.yml
@@ -5,6 +5,14 @@ name: Build and Test Suite
 # NOTE: All test projects now use 'dotnet test' consistently.
 # This was updated to fix issues with direct executable execution that were
 # causing "No such file or directory" errors in CI.
+#
+# NOTE: The "Build Solution" job is a required status check on main's
+# ruleset, so this workflow itself still triggers on every pull_request
+# (no paths-ignore/paths filter on the trigger - see PR #26). Instead, the
+# "changes" job below detects whether src/tests/project files actually
+# changed and the downstream jobs are skipped via `if:` when they didn't.
+# GitHub treats a skipped required check as passing, so docs/config-only
+# PRs still merge cleanly without paying for a full build+test run.
 
 permissions:
   issues: write
@@ -26,10 +34,8 @@ permissions:
     paths-ignore:
       - "docs/**"
 
-  # No paths-ignore here: the "Build Solution" job below is a required
-  # status check on main's ruleset. A docs-only PR that this workflow
-  # never runs against would leave that check permanently missing,
-  # blocking the PR from merging forever (see PR #26).
+  # No paths-ignore here: see the NOTE at the top of this file for why
+  # the trigger stays broad while the "changes" job gates the actual work.
   pull_request:
     types: [opened, synchronize, reopened, ready_for_review]
 
@@ -68,10 +74,49 @@ env:
     ${{ secrets.AUTH0_MANAGEMENT_CLIENT_SECRET }}
 
 jobs:
+  changes:
+    name: Detect Code Changes
+    runs-on: ubuntu-latest
+    timeout-minutes: 5
+    outputs:
+      code: ${{ steps.force.outputs.run == 'true' || steps.filter.outputs.code == 'true' }}
+
+    steps:
+      - name: Checkout code
+        uses: actions/checkout@v7
+
+      - name: Force run on manual/reusable triggers
+        id: force
+        run: |
+          if [[ "${{ github.event_name }}" == "workflow_dispatch" || "${{ github.event_name }}" == "workflow_call" ]]; then
+            echo "run=true" >> "$GITHUB_OUTPUT"
+          else
+            echo "run=false" >> "$GITHUB_OUTPUT"
+          fi
+
+      - name: Check for src/test changes
+        id: filter
+        if: steps.force.outputs.run == 'false'
+        uses: dorny/paths-filter@v3
+        with:
+          filters: |
+            code:
+              - 'src/**'
+              - 'tests/**'
+              - '**/*.csproj'
+              - '**/*.slnx'
+              - 'Directory.Build.props'
+              - 'Directory.Packages.props'
+              - 'global.json'
+              - '.github/workflows/ci.yml'
+
   build:
     name: Build Solution
     runs-on: ubuntu-latest
     timeout-minutes: 15
+    needs:
+      - changes
+    if: needs.changes.outputs.code == 'true'
 
     steps:
       - name: Checkout code
@@ -293,11 +338,14 @@ jobs:
           if-no-files-found: ignore
 
   coverage:
-    if: ${{ always() && github.actor != 'dependabot[bot]' }}
+    if: >-
+      ${{ always() && needs.changes.outputs.code == 'true' &&
+      github.actor != 'dependabot[bot]' }}
     name: Coverage Analysis
     runs-on: ubuntu-latest
     timeout-minutes: 10
     needs:
+      - changes
       - build
       - discover-tests
       - test
@@ -376,11 +424,14 @@ jobs:
           verbose: true
 
   report:
-    if: ${{ always() && github.actor != 'dependabot[bot]' }}
+    if: >-
+      ${{ always() && needs.changes.outputs.code == 'true' &&
+      github.actor != 'dependabot[bot]' }}
     name: Test Report Summary
     runs-on: ubuntu-latest
     timeout-minutes: 10
     needs:
+      - changes
       - build
       - discover-tests
       - test
```