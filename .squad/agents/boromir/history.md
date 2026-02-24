# Project Context

- **Owner:** Matthew Paulosky
- **Project:** IssueManager — issue management app with .NET 10, Blazor, MongoDB, CQRS, Vertical Slice Architecture
- **Stack:** .NET 10, C# 14, Blazor (server-side), Aspire, MongoDB.EntityFrameworkCore, MediatR (CQRS), FluentValidation, xUnit, FluentAssertions, NSubstitute, bUnit, TestContainers, GitHub Actions
- **My Role:** DevOps / Infrastructure Engineer — GitHub Actions, CI/CD, Aspire, Docker, builds, releases
- **Key Paths:** `.github/workflows/`, `src/AppHost/`, `scripts/`, `Directory.Packages.props`, `global.json`
- **Created:** 2026-02-24

## Learnings

### Established Process (inherited from prior sessions)

- Feature branches mandatory for all changes — direct pushes to main are prohibited
- Build-repair skill at `.squad/skills/build-repair/SKILL.md` must run before any push
- Release automation implemented via `.github/release.yml` + `squad-release.yml` (tag-triggered, `--generate-notes`)
- Tests renamed: `tests/Unit.Tests/`, `tests/Architecture.Tests/`, `tests/Blazor.Tests/`, `tests/Integration.Tests/`
- Integration tests use TestContainers + MongoDB 8.0 — Docker must be running, tests take ~105s

<!-- Append new learnings below. -->
