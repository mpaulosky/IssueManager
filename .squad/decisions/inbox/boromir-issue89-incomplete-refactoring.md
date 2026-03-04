# Issue #89 — Aspire Startup Fixes: Incomplete Refactoring Blocked Commit

## Summary
When attempting to commit Matthew's startup fixes for Issue #89, discovered that the working tree contains incomplete ObjectId type refactoring (appears to be related to squad/80-foundation-objectid-standardization branch) alongside the Aspire startup configuration fixes.

## Problem
- **Build Status:** FAILED with 14+ compilation errors
- **Error Pattern:** Type mismatch errors across multiple files
  - `ObjectId` properties initialized with `string.Empty` (type incompatibility)
  - Handlers using `string` parameters where `ObjectId` is expected and vice versa
  - Web pages passing `string` to APIs expecting `ObjectId`

## Files with Errors
- Shared validators (9 files): ObjectId type initialization issues
- API handlers: CategoryEndpoints, StatusEndpoints, CommentEndpoints, UpdateIssueStatusHandler
- Web pages: EditIssuePage, EditCategoryPage, EditStatusPage, IssueDetailPage

## Root Cause
The ObjectId refactoring is incomplete:
1. DTO/Command record types changed from `string` to `ObjectId`
2. BUT endpoint handlers, services, and pages still use `string` parameters
3. Type conversions not implemented across the layer boundary
4. This is a pervasive application-logic refactoring, NOT just startup configuration

## Scope Issue
This work is beyond DevOps responsibility:
- **DevOps owns:** AppHost orchestration, ServiceDefaults, NuGet, CI/CD configuration
- **Sam/Aragorn own:** Application logic, handlers, services, page components
- **Gimli owns:** Test code updates

The ObjectId refactoring requires coordinated changes across all layers by application developers.

## Recommendation
**Action Required by Matthew / Aragorn / Sam:**
1. Complete the ObjectId refactoring (coordinate across all layers)
2. Ensure all type conversions are consistent (string → ObjectId, ObjectId → string via .ToString())
3. Run `dotnet build` locally to validate before pushing
4. Then re-request the Aspire startup fixes commit

**For Now:**
- Do NOT merge incomplete application changes
- Separate concerns: pure Aspire infrastructure fixes can be committed independently
- The AppHost, ServiceDefaults, and API startup configuration changes are sound (tested in isolation)
