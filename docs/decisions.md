# Architecture Decisions

This document records durable architectural and design decisions for the IssueManager
repository — the choices a developer or AI agent working on this codebase should know
about to avoid re-litigating settled questions or re-discovering non-obvious behavior.
It is a flat, dated log rather than a numbered ADR series. Routine dependency bumps,
test-migration mechanics, and sprint status reports are intentionally left out; that
history lives in `docs/CHANGELOG.md` and git log instead.

---

### 2026-02-25: NuGet package versions centralized in Directory.Packages.props

All NuGet package versions are managed centrally in `Directory.Packages.props` at the
repo root. Individual `.csproj` files must not specify package versions. This prevents
version drift across projects and simplifies upgrades to a single file.

### 2026-02-25: IssueDto.Empty and CommentDto.Empty are not singletons

`IssueDto.Empty` (and `CommentDto.Empty`) is a static property, not a field — it calls
`DateTime.UtcNow` on every access, so each access produces a new instance with a
different timestamp. Code and tests must never compare an instance against
`IssueDto.Empty` as if it were a stable sentinel value; always assert on individual
fields instead.

### 2026-02-25: GenerateSlug's trailing underscore is intentional

`GenerateSlug` appends a trailing `_` when the input string both ends with a
non-alphanumeric character and contains at least one other internal non-alphanumeric
(non-space) character. This is correct, intentional behavior, not a bug — for example
`"C# Is Great!"` slugifies to `"c_is_great_"`. Anyone touching this helper or writing
tests against it should match the actual output rather than "fixing" it.

### 2026-02-26: Repository pattern — the interface is the contract

When a repository interface and its implementation/callers disagree, the interface is
authoritative: implementations and callers are updated to match the interface, never
the reverse. This keeps a single source of truth for repository contracts and avoids
signature drift between interfaces and handlers.

### 2026-02-27: MongoDB search/filter pattern via Builders\<T\>.Filter

List/search endpoints that support optional filtering follow a consistent pattern
using MongoDB's `Builders<T>.Filter` API: start from required base filters (e.g.
`Archived == false`), add optional filters only when their corresponding parameter is
non-null/non-empty, use case-insensitive `BsonRegularExpression` for text search, and
combine everything with `Filter.And()`. New filterable list endpoints should follow
this same shape rather than inventing a new one.

### 2026-02-27: Auth0 uses a passive-configuration pattern

Auth0 authentication extensions check for required configuration (domain, client
ID/audience) before wiring themselves up. If configuration is missing, they return
early without throwing, and the application runs in "open mode" with no authentication
enforced. This is intentional graceful degradation, not a security bug — it exists so
the app keeps building and running while Auth0 secrets are being provisioned.

### 2026-02-27: CurrentUserService reads Auth0 JWT claims with a fallback strategy

`ICurrentUserService` exposes the authenticated user's identity (UserId, Name, Email,
IsAuthenticated) by reading claims from `HttpContext.User`. It tries the standard
.NET claim types first (`ClaimTypes.NameIdentifier`/`Name`/`Email`) and falls back to
Auth0's own claim names (`sub`/`name`/`email`) if the standard ones aren't present,
and handles unauthenticated requests gracefully rather than throwing.

### 2026-02-28: API versioning strategy

The API uses `Asp.Versioning.Http` with a default version of 1.0, assumes the default
version when a client doesn't specify one, and reports supported versions in response
headers. Clients may select a version via URL segment, an `X-Api-Version` header, or an
`api-version` query string parameter. Existing `/api/v1/` routes continue to work
unchanged.

### 2026-02-28: Project confirmed non-commercial

IssueManager is confirmed to be a non-commercial project. This is a standing licensing
directive: dependencies whose free tier excludes commercial use (e.g. FluentAssertions
v7+) may be adopted without a licensing review being triggered by this project's usage.

### 2026-03-03: ObjectId parsing at the API boundary, Result\<T\> throughout

IDs arrive from clients as strings and are parsed to `ObjectId` before reaching handler
business logic — handler bodies never call `ObjectId.TryParse()` themselves. Commands
and queries hold strongly-typed `ObjectId` properties (never `string`, never
`ObjectId?`), and Blazor pages parse the string route parameter to `ObjectId` before
constructing a command. Separately, all API handlers return `Task<Result<T>>` rather
than raw DTOs, bools, or throwing exceptions; repositories return `Result<T>`
internally and handlers unwrap/re-wrap it. Endpoints map the `Result<T>` outcome to the
appropriate HTTP status code (404 for not-found, 409 for conflict, 400 for validation
failure, etc.). Together these give fail-fast validation at the boundary and consistent,
type-safe error handling from repository to HTTP response.

### 2026-03-04: Auth0 roles require explicit claim mapping

Role-based authorization (`[Authorize(Roles = "Admin")]`, `<AuthorizeView Roles="Admin">`)
depends on Auth0 including a roles claim in the JWT and on `AuthExtensions.cs` mapping
that namespaced claim to `ClaimTypes.Role`. Without this mapping, `User.IsInRole(...)`
silently always returns `false` and every Admin-gated page silently denies access — it
does not throw or log an error, so this is easy to misdiagnose as an authorization bug
rather than a missing claim mapping.

### 2026-03-06: Web project uses Vertical Slice Architecture

The `src/Web` project is organized by feature slice rather than by horizontal layer:
each feature owns its own folder containing its pages, components, and related code,
instead of being split across separate `Pages/`, `Components/`, `Services/` layers.
New Web features should follow the same self-contained-slice convention.

### 2026-03-10: Direct pushes to main are blocked

The pre-push hook blocks direct pushes to `main` (or `master`). All work must go
through a feature branch and a pull request. This was made an enforced gate after a
direct push to main bypassed the PR/review process.

### 2026-03-10: Create handlers generate ObjectIds; repositories validate them

For all `Create` operations, the handler is responsible for generating a new ID via
`ObjectId.GenerateNewId()` when constructing the DTO/model being persisted — it does
not rely on the repository or MongoDB to generate the ID. Repositories, in turn,
validate that an incoming ID is not `ObjectId.Empty` before performing database
operations. This gives explicit ID ownership at the application layer and makes
"forgot to set an ID" a testable, fail-fast error rather than a silent database
surprise. Any new Create handler should follow this same generate-in-handler,
validate-in-repository split.

### 2026-04-15: Soft-delete architecture for Categories and Statuses

Categories and Statuses use soft delete rather than hard delete: "deleting" one sets
an `IsArchived` flag instead of removing the row. Issues that reference an archived
Category or Status keep their association, but archived Categories/Statuses are
excluded from active selection UI. This preserves historical/referential integrity for
issues created against a Category or Status that is later retired.

### 2026-08-27: Remove squad-team framework

The repository switched to Claude Code for AI-assisted development, which does not
interface with the squad-team framework (its agent charters, ceremonies, casting
system, and MCP-based state store). Keeping the `.squad/` framework around after that
switch is dead weight rather than active infrastructure. This repository's removal of
`.squad/` doubles as a validation run for a reusable removal runbook that will be
applied to other repositories still running squad. The full squad-era decision log and
session trail are not lost — they remain available in git history if ever needed;
this document instead carries forward only the durable architectural decisions worth
keeping in front of future work.

### 2026-08-27: Result\<T\> is always returned, never bypassed by throwing, and maps to HTTP through one place

The existing rule that all API handlers return `Task<Result<T>>` (see the 2026-03-03
entry) is absolute: a handler must never throw for an expected failure case (validation,
not-found, conflict). `ResultExtensions.ToHttpResult()` in `src/Api/Extensions/` is the
single place that maps a `Result`/`Result<T>` outcome to an HTTP response, and every
route in every `*Endpoints.cs` file uses it rather than hand-rolling its own
`result.Success ? ... : ...` branch. The canonical `ResultErrorCode` → status mapping is:
`NotFound` → 404, `Validation` → 400, `Conflict` → 409, `Concurrency` → 409 (the
optimistic-concurrency version detail travels in `Result.Details` for callers that want
it, rather than warranting a distinct status code like 412). A failed `Result` carrying
`ResultErrorCode.None` is a contract violation, not a client error — `ToHttpResult()`
throws rather than silently returning 400, so a handler that forgets to set a real error
code fails loudly instead of shipping a wrong status code. FluentValidation failures are
collapsed to a single message string (matching every other handler's `Result.Error`
shape) rather than preserved as structured per-field errors. List endpoints that have no
validatable input (Categories, Statuses, Comments) are not wrapped in `Result<T>` just
for uniformity; only `ListIssuesHandler` needs it, because it's the only one with
parameters that can actually fail validation.

### 2026-08-27: New MongoDB-backed entities inherit MongoRepository, not a hand-written CRUD class

`MongoRepository<TModel, TDto>` in `src/Api/Data/` implements the shared CRUD contract
(`IRepository<TDto>`: archive, create, get-by-id, get-all, update, count) once. A new
entity's concrete repository (e.g. `CategoryRepository`) should inherit
`MongoRepository<TModel, TDto>`, pass its collection name and entity name to the base
constructor, override `ToDto`/`ToModel` with one-line calls to the entity's existing
static mapper (`src/Shared/Mappers/`), and add only its own genuinely distinctive query
methods (filtered listings, lookups, vote/relationship operations) — it should never
reimplement the 5 shared CRUD methods by hand. The entity's model class must implement
`IEntity` (`Shared.Abstractions`) — just `ObjectId Id` and `bool Archived`, which every
model already has — so the generic base can filter by id and set the archived flag
without knowing the concrete model type. The entity's own repository interface
(`ICategoryRepository`, etc.) should extend `IRepository<TDto>` rather than
re-declaring the 5 shared method signatures.
