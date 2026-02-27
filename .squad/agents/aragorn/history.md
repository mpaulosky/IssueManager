# Aragorn — History

## Core Context
Lead Developer on IssueManager (.NET 10, Blazor, MongoDB, CQRS, VSA). User: Matthew Paulosky. Repo: mpaulosky/IssueManager.

## Learnings

### 2026-02-25: Build Repair Run
- Ran full restore/build/test cycle on main: 378 tests passing, 0 errors, 0 warnings
- Committed `[Collection("Integration")]` attributes on 7 handler test files + IntegrationTestCollection.cs
- Opened PR #54 on branch `feature/build-repair-20260225`

### 2026-02-25: CI failures on PR #54 — root causes fixed
- NuGet.config had Windows-only `%USERPROFILE%\.nuget\packages_aspire` path — removed `<config>` block
- `.squad/` files were in PR diff on a `feature/*` branch — Protected Branch Guard blocked it
- Fixed: `git rm --cached -r .squad/` to untrack squad files from the feature branch
- Fix commit: `26b3e73` — PR #54 merged as `81aef45`

### 2026-02-25: Squad state recovery
- Squash-merge of PR #54 wiped `.squad/` from local disk (git rm --cached had untracked them)
- Recovery path: `git show {commit}:.squad/{path}` to restore from git object store
- Prevention: always commit `.squad/` state on `squad/*` branches, never `feature/*`

### 2026-02-26: Build Repair — Interface/Implementation Mismatch
- **Problem**: `src\Api\Api.csproj` had 14 build errors due to handler code calling wrong repository methods
- **Root cause**: Handlers were calling old method signatures (`GetAsync`, `UpdateAsync(objectId, dto)`) while interfaces had been updated to new signatures (`GetByIdAsync`, `UpdateAsync(dto)`)
- **Pattern**: Create handlers passed `Model` objects instead of `Dto` to `CreateAsync`; Update/Delete handlers called `GetAsync` instead of `GetByIdAsync`; Archive methods called with DTO instead of ObjectId
- **Fixes applied**:
	- Category handlers: Updated to use `GetByIdAsync`, create `CategoryDto` directly, pass DTOs to repository methods
	- Status handlers: Updated to use `GetByIdAsync`, create `StatusDto` directly, removed redundant `.ToDto()` call in ListStatusesHandler
	- Comment handlers: Updated to use `GetByIdAsync`, create `CommentDto` directly with all required fields
	- Issue handlers: Fixed `DeleteIssueHandler` to parse string ID to `ObjectId` and work with `Result<IssueDto>` return type
- **Result**: Build exits with code 0, 14 nullable warnings (acceptable), test project builders have separate issues outside scope
- **Key learning**: Repository interfaces define the contract — always update implementations and callers to match the interface, never the reverse

### Key File Paths
- Solution: `E:\github\IssueManager\IssueManager.sln`
- API source: `src/Api/`
- Shared project: `src/Shared/`
- Tests: `tests/Unit.Tests/`, `tests/Integration.Tests/`, `tests/Architecture.Tests/`, `tests/Web.Tests.Bunit/`
- Squad skills: `.squad/skills/`
- Build repair prompt: `.github/prompts/build-repair.prompt.md`

---

## 2026-02-27 05:20:20 - Integration Test Compilation Repair

**Task:** Fix pre-existing integration test compilation failures

**Issues Fixed:**
1. IssueDto constructor parameter mismatches (12 parameters required)
2. Result<T> wrapper handling - all repository methods now return Result<T>
3. ObjectId vs string type mismatches in method signatures
4. Result tuple deconstruction issues for GetAllAsync pagination

**Files Modified:**
- tests\Integration.Tests\Data\IssueRepositoryTests.cs
- tests\Integration.Tests\Builders\IssueBuilder.cs
- tests\Integration.Tests\Handlers\CreateIssueHandlerTests.cs
- tests\Integration.Tests\Handlers\DeleteIssueHandlerTests.cs
- tests\Integration.Tests\Handlers\DeleteIssueHandlerIntegrationTests.cs
- tests\Integration.Tests\Handlers\GetIssueHandlerTests.cs
- tests\Integration.Tests\Handlers\ListIssuesHandlerIntegrationTests.cs
- tests\Integration.Tests\Handlers\UpdateIssueHandlerIntegrationTests.cs
- tests\Integration.Tests\Handlers\UpdateIssueStatusHandlerTests.cs

**Result:** Build succeeded with 0 errors, unit tests passing
