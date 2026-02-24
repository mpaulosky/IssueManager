# Project Context

- **Owner:** Matthew Paulosky
- **Project:** IssueManager — issue management app with .NET 10, Blazor, MongoDB, CQRS, Vertical Slice Architecture
- **Stack:** .NET 10, C# 14, MongoDB.EntityFrameworkCore, MongoDB.Driver, EF Core, MediatR, FluentValidation, TestContainers, xUnit
- **My Role:** Backend Data Engineer — MongoDB persistence, repositories, integration tests, data models
- **Key Paths:** `src/Persistence.MongoDb/`, `src/Api/Repositories/`, `src/Shared/Models/`, `tests/Integration.Tests/`
- **Created:** 2026-02-24

## Learnings

### Repository & Data Patterns (inherited from prior sessions)

- All repositories use `Result<T>` return pattern — never throw for expected failures
- `IDbContextFactory<T>` for DbContext — never inject directly into singletons
- Integration tests use TestContainers with MongoDB 8.0 — require Docker running
- Repositories: IssueRepository, CategoryRepository, CommentRepository, StatusRepository
- ObjectId validation done in repositories (IssueRepository.GetByIdAsync checks ObjectId.TryParse)
- Repository unit-testable logic: ObjectId validation (Issue), null checks (all), userId validation (Comment)

<!-- Append new learnings below. -->
