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

### Key File Paths
- Solution: `E:\github\IssueManager\IssueManager.sln`
- API source: `src/Api/`
- Shared project: `src/Shared/`
- Tests: `tests/Unit.Tests/`, `tests/Integration.Tests/`, `tests/Architecture.Tests/`, `tests/Web.Tests.Bunit/`
- Squad skills: `.squad/skills/`
- Build repair prompt: `.github/prompts/build-repair.prompt.md`
