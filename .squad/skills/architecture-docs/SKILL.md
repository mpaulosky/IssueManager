# Architecture Documentation — Comprehensive System Design Reference

**Confidence:** `low`  
**Last Updated:** 2026-02-24  
**Owner:** Frodo (Technical Writer / DevRel)

---

## Purpose

Document the system architecture comprehensively, making design patterns, data flows, and layer boundaries explicit for new developers, contributors, and future maintainers. Ensures architecture is captured in prose, diagrams, and decision rationale — not just in code.

## Architecture Overview

### Pattern: Vertical Slice Architecture + CQRS

The IssueManager applies **Vertical Slice Architecture** where each business feature (Issues, Comments, Categories, Statuses) owns its complete stack:

```
Feature Slice (e.g., "Issues")
├── Web Layer (Blazor page/component)
├── Handler (CQRS command/query logic)
├── Validator (FluentValidation rules)
├── Mapper (DTO ↔ Model conversion)
├── Repository (data access abstraction)
└── Models & DTOs (data contracts)
```

**CQRS Pattern:** Commands modify state (Create, Update, Delete). Queries read state (Get, List). Handlers are named to reflect intent: `CreateIssueHandler`, `ListIssuesHandler`, `UpdateIssueStatusHandler`.

### Layer Boundaries (Enforced by NetArchTest)

```
┌─────────────────────────────────────────────────────┐
│ Web (Blazor SSR)                                    │
│ ├─ Pages (Blazor pages ending in .razor)          │
│ └─ Components (Blazor components)                   │
│ (depends on: Shared, Api)                          │
└──────────────────┬──────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────┐
│ Api (Business Logic & Data Access)                  │
│ ├─ Handlers (CQRS: CreateXyzHandler, etc.)         │
│ ├─ Repositories (MongoDB abstraction)              │
│ ├─ Mappers (DTO ↔ Model conversion)               │
│ └─ Validators (FluentValidation rules)             │
│ (depends on: Shared)                               │
└──────────────────┬──────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────┐
│ Shared (Domain Models & Contracts)                  │
│ ├─ Models (Issue, Comment, Category, Status)       │
│ ├─ DTOs (IssueDto, CommentDto, etc.)              │
│ ├─ Exceptions (NotFoundException, etc.)            │
│ ├─ Abstractions (Result<T>, IRepository)           │
│ └─ Validators (shared validation rules)            │
│ (depends on: nothing except external packages)     │
└─────────────────────────────────────────────────────┘
```

**Critical Rules:**
1. **Shared has zero dependencies** on Api or Web (enforced by NetArchTest)
2. **Api depends only on Shared** (no Web, no ServiceDefaults, no MongoDB drivers leaked)
3. **Web depends only on Shared + Api** (no direct repository access)
4. **Tests depend only on their tested layer** (Unit tests ≠ Integration tests)

## CQRS Handler Pattern

### Command Handler Example: CreateIssue

**Handler Location:** `src/Api/Handlers/Issues/CreateIssueHandler.cs`

```csharp
public class CreateIssueHandler
{
  private readonly IIssueRepository _repository;
  private readonly IssueValidator _validator;
  
  public CreateIssueHandler(IIssueRepository repository, IssueValidator validator)
  {
    _repository = repository;
    _validator = validator;
  }
  
  public async Task<Result<IssueDto>> Handle(CreateIssueCommand command)
  {
    // 1. Validate input
    var validationResult = _validator.Validate(command);
    if (!validationResult.IsValid)
      return Result<IssueDto>.Fail(validationResult.ToString());
    
    // 2. Create domain model
    var issue = new Issue(
      ObjectId.GenerateNewId(),
      command.Title,
      command.Description,
      DateTime.UtcNow,
      command.Author,     // UserDto
      command.Category,   // CategoryDto
      command.Status);    // StatusDto
    
    // 3. Persist
    var created = await _repository.CreateAsync(issue);
    if (!created)
      return Result<IssueDto>.Fail("Failed to create issue");
    
    // 4. Map to DTO and return
    var dto = IssueMapper.ToDto(issue);
    return Result<IssueDto>.Ok(dto);
  }
}
```

**Pattern Rules:**
- **Single Responsibility:** Handler coordinates only — doesn't do validation, mapping, or direct DB ops
- **Dependency Injection:** All dependencies injected via constructor
- **Result<T> Pattern:** All handlers return `Result<T>` for error handling
- **Async/Await:** All I/O operations are async (`Task<T>`)
- **Immutable DTOs:** Returned DTOs are read-only for consumers

### Query Handler Example: ListIssues

**Handler Location:** `src/Api/Handlers/Issues/ListIssuesHandler.cs`

```csharp
public class ListIssuesHandler
{
  private readonly IIssueRepository _repository;
  
  public ListIssuesHandler(IIssueRepository repository)
  {
    _repository = repository;
  }
  
  public async Task<Result<PaginatedResponse<IssueDto>>> Handle(ListIssuesQuery query)
  {
    var (issues, total) = await _repository.ListAsync(
      pageNumber: query.PageNumber,
      pageSize: query.PageSize);
    
    var dtos = issues.Select(IssueMapper.ToDto).ToList();
    return Result<PaginatedResponse<IssueDto>>.Ok(
      new PaginatedResponse<IssueDto>(dtos, total));
  }
}
```

**Pattern Rules:**
- **No validation for queries** (queries return what exists, not what's valid)
- **Tuple returns:** Repositories return `(items, totalCount)` for pagination
- **DTO mapping:** Results always mapped to DTOs before returning to caller

## Repository Pattern (MongoDB Abstraction)

### Repository Interface (Shared Layer)

**Location:** `src/Shared/Abstractions/IRepository.cs` (generic) + `src/Shared/Abstractions/IIssueRepository.cs` (specific)

```csharp
// Generic interface
public interface IRepository<T> where T : class
{
  Task<T?> GetByIdAsync(string id);
  Task<(List<T> Items, int Total)> ListAsync(int pageNumber, int pageSize);
  Task<bool> CreateAsync(T entity);
  Task<bool> UpdateAsync(T entity);
  Task<bool> DeleteAsync(string id);
  Task<bool> ArchiveAsync(string id);
}

// Specific interface (adds domain-specific queries)
public interface IIssueRepository : IRepository<Issue>
{
  Task<List<Issue>> GetByCategoryAsync(string categoryId);
  Task<List<Issue>> GetByStatusAsync(string statusId);
}
```

**Why this pattern:**
1. **Abstraction:** Decouples handlers from MongoDB specifics (could swap to EF Core, SQL Server, etc.)
2. **Testability:** Can mock repositories in unit tests without database
3. **Consistency:** All repositories follow same interface contract

### Repository Implementation (Api Layer)

**Location:** `src/Api/Data/Repositories/IssueRepository.cs`

```csharp
public class IssueRepository : IIssueRepository
{
  private readonly IMongoCollection<Issue> _collection;
  
  public IssueRepository(IMongoDatabase database)
  {
    _collection = database.GetCollection<Issue>("issues");
  }
  
  public async Task<Issue?> GetByIdAsync(string id)
  {
    if (!ObjectId.TryParse(id, out var objectId))
      return null;
    
    return await _collection.Find(i => i.Id == objectId).FirstOrDefaultAsync();
  }
  
  public async Task<(List<Issue> Items, int Total)> ListAsync(int pageNumber, int pageSize)
  {
    var skip = (pageNumber - 1) * pageSize;
    
    var items = await _collection
      .Find(Builders<Issue>.Filter.Empty)
      .Skip(skip)
      .Limit(pageSize)
      .ToListAsync();
    
    var total = (int)await _collection.EstimatedDocumentCountAsync();
    
    return (items, total);
  }
  
  public async Task<bool> CreateAsync(Issue entity)
  {
    try
    {
      await _collection.InsertOneAsync(entity);
      return true;
    }
    catch
    {
      return false;
    }
  }
}
```

**Pattern Rules:**
- **Validation in repository:** Input validation (ObjectId.TryParse, null checks) before DB calls
- **Error handling:** Catch exceptions, return bool/null instead of throwing
- **Async MongoDB:** Use `Async` variants of all MongoDB operations
- **Result tuples:** Return `(items, total)` for pagination; never return incomplete data

## Data Flow: Web → Api → Repository → MongoDB

```
┌─────────────────────────────────────────────────────────────────┐
│ Blazor Component (Web/Components/IssueList.razor)              │
│ • Displays issues in table                                      │
│ • Fires: "Create Issue" button                                  │
└────────────────────────┬────────────────────────────────────────┘
                         │ User clicks "Create"
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ Blazor Page (Web/Pages/CreateIssuePage.razor)                  │
│ • Form: Title, Description, Author, Category, Status           │
│ • Validates form locally                                        │
│ • Calls: await mediator.Send(new CreateIssueCommand(...))     │
└────────────────────────┬────────────────────────────────────────┘
                         │ POST /api/issues
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ Api Endpoint (minimal API or controller)                       │
│ • Maps form → CreateIssueCommand                                │
│ • Calls: handler.Handle(command)                                │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ CreateIssueHandler (Api/Handlers/Issues/)                       │
│ • Validates CreateIssueCommand (FluentValidation)               │
│ • Creates Issue model                                           │
│ • Calls: _repository.CreateAsync(issue)                         │
│ • Returns: Result<IssueDto>                                     │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ IssueRepository (Api/Data/Repositories/)                        │
│ • Validates: ObjectId.TryParse(id)                              │
│ • Calls: _collection.InsertOneAsync(issue)                      │
│ • Returns: bool (success/failure)                               │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ MongoDB Collection ("issues")                                   │
│ • Stores: Issue document with all fields                        │
└─────────────────────────────────────────────────────────────────┘
```

## DTO ↔ Model Mapping Pattern

### Why Separate DTOs from Models?

- **Models:** Domain logic, business rules, state management (internal)
- **DTOs:** Data contracts, API contracts, serializable (external)

### Mapper Pattern

**Location:** `src/Api/Mappers/IssueMapper.cs`

```csharp
public static class IssueMapper
{
  public static IssueDto ToDto(Issue model)
  {
    return new IssueDto(
      model.Id.ToString(),
      model.Title,
      model.Description,
      model.DateCreated,
      UserMapper.ToDto(model.Author),
      CategoryMapper.ToDto(model.Category),
      StatusMapper.ToDto(model.Status),
      model.Archived,
      model.ArchivedBy is not null ? UserMapper.ToDto(model.ArchivedBy) : UserDto.Empty,
      model.DateModified);
  }
  
  public static Issue ToModel(IssueDto dto, UserDto author, CategoryDto category, StatusDto status)
  {
    return new Issue(
      ObjectId.Parse(dto.Id),
      dto.Title,
      dto.Description,
      dto.DateCreated,
      author,
      category,
      status);
  }
}
```

**Pattern Rules:**
- **One-directional or two-directional:** Specify clearly in docs
- **Null handling:** Explicit (`.IsNullOrEmpty ? Default : Map`)
- **Type conversion:** ObjectId ↔ string conversions happen here
- **Immutability:** Mappers produce immutable DTOs

## Validation Pattern (FluentValidation)

### Validator Location & Naming

**Location:** `src/Api/Validators/{Domain}Validator.cs`  
**Pattern:** One validator per domain entity

```csharp
public class IssueValidator : AbstractValidator<CreateIssueCommand>
{
  public IssueValidator()
  {
    RuleFor(x => x.Title)
      .NotEmpty().WithMessage("Title is required")
      .MinimumLength(3).WithMessage("Title must be at least 3 characters")
      .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");
    
    RuleFor(x => x.Description)
      .MaximumLength(5000).WithMessage("Description cannot exceed 5000 characters");
    
    RuleFor(x => x.Category)
      .NotNull().WithMessage("Category is required");
  }
}
```

**Pattern Rules:**
- **Command validation:** One validator per command/query (not per model)
- **Clear error messages:** User-facing, actionable
- **Shared validators:** Can be reused in handlers via dependency injection
- **Early validation:** Validators run BEFORE repository operations

## Architecture Enforcement (NetArchTest)

### What Gets Enforced?

**Location:** `tests/Architecture.Tests/ArchitectureTests.cs`

```csharp
[Fact]
public void Shared_ShouldNotDependOn_ApiOrWeb()
{
  var shared = Types.InNamespace("IssueManager.Shared");
  var api = Types.InNamespace("IssueManager.Api");
  var web = Types.InNamespace("IssueManager.Web");
  
  shared.Should()
    .NotDependOnAny(api, web);
}

[Fact]
public void Api_ShouldNotDependOn_Web()
{
  var api = Types.InNamespace("IssueManager.Api");
  var web = Types.InNamespace("IssueManager.Web");
  
  api.Should()
    .NotDependOnAny(web);
}

[Fact]
public void Validators_ShouldInheritFrom_AbstractValidator()
{
  var validators = Types.InNamespace("IssueManager.Api.Validators")
    .That()
    .HaveNameEndingWith("Validator");
  
  validators.Should()
    .Inherit(typeof(AbstractValidator<>));
}
```

**Why NetArchTest?**
- **Compile-time enforcement:** Violations fail tests, blocking CI
- **Documentation:** Tests ARE the architecture contract
- **Prevents drift:** Can't accidentally break layering rules

## File Structure Reference

```
src/
├── IssueManager.Web/                    # Blazor UI (Server-Side Rendering)
│   ├── Pages/                           # Blazor pages (@page routes)
│   │   ├── IssuePage.razor
│   │   └── CreateIssuePage.razor
│   ├── Components/                      # Reusable Blazor components
│   │   ├── IssueList.razor
│   │   └── CategorySelector.razor
│   ├── Program.cs                       # DI registration, middleware setup
│   └── appsettings.json
│
├── IssueManager.Api/                    # Business Logic & Data Access
│   ├── Handlers/                        # CQRS handlers
│   │   ├── Issues/
│   │   │   ├── CreateIssueHandler.cs
│   │   │   ├── GetIssueHandler.cs
│   │   │   ├── ListIssuesHandler.cs
│   │   │   ├── UpdateIssueStatusHandler.cs
│   │   │   └── DeleteIssueHandler.cs
│   │   ├── Categories/                  # Similar structure
│   │   ├── Statuses/
│   │   └── Comments/
│   │
│   ├── Data/                            # MongoDB data access
│   │   └── Repositories/
│   │       ├── IssueRepository.cs
│   │       ├── CategoryRepository.cs
│   │       ├── StatusRepository.cs
│   │       └── CommentRepository.cs
│   │
│   ├── Mappers/                         # DTO ↔ Model conversion
│   │   ├── IssueMapper.cs
│   │   ├── CategoryMapper.cs
│   │   ├── StatusMapper.cs
│   │   ├── CommentMapper.cs
│   │   └── UserMapper.cs
│   │
│   ├── Validators/                      # FluentValidation rules
│   │   ├── IssueValidator.cs
│   │   ├── CategoryValidator.cs
│   │   ├── StatusValidator.cs
│   │   └── CommentValidator.cs
│   │
│   └── Program.cs                       # DI registration for Api layer
│
├── IssueManager.Shared/                 # Domain Models & Contracts
│   ├── Models/                          # Domain entities
│   │   ├── Issue.cs
│   │   ├── Category.cs
│   │   ├── Status.cs
│   │   └── Comment.cs
│   │
│   ├── Dtos/                            # Data transfer objects
│   │   ├── IssueDto.cs
│   │   ├── CategoryDto.cs
│   │   ├── StatusDto.cs
│   │   └── CommentDto.cs
│   │
│   ├── Abstractions/                    # Interfaces & base types
│   │   ├── IRepository.cs
│   │   ├── IIssueRepository.cs
│   │   ├── Result.cs
│   │   └── Utilities.cs
│   │
│   ├── Exceptions/                      # Custom exceptions
│   │   ├── NotFoundException.cs
│   │   ├── ConflictException.cs
│   │   └── ValidationException.cs
│   │
│   └── GlobalUsings.cs                  # Global using statements
│
├── IssueManager.ServiceDefaults/        # Cross-cutting infrastructure
│   ├── Extensions/                      # OpenTelemetry, Aspire, middleware
│   │   ├── AspireExtensions.cs
│   │   └── LoggingExtensions.cs
│   └── Program.cs
│
└── IssueManager.AppHost/                # Aspire orchestration
    ├── Program.cs                       # Defines services & connections
    └── appsettings.json
```

## Key Architectural Decisions (ADRs)

### 1. Vertical Slice > Horizontal Layers

**Decision:** Organize by feature, not technical layer.

**Rationale:** 
- Reduces cognitive load (related code is together)
- Makes testing easier (can test entire slice)
- Simplifies onboarding (find all Issue logic in one place)

**Impact:** 
- Breaking changes are localized to the slice
- New slices follow established pattern

### 2. CQRS Simplicity (No Mediatr)

**Decision:** Handlers are plain classes, no MediatR library.

**Rationale:**
- Less magic, more transparency
- Easier debugging (simple method calls)
- Dependencies are explicit (constructor injection)

**Impact:**
- Handlers must be registered in DI explicitly
- Call handlers directly: `await handler.Handle(command)`

### 3. Repository Pattern (MongoDB Abstraction)

**Decision:** Repositories abstract MongoDB behind interface.

**Rationale:**
- Decouples business logic from database technology
- Enables unit testing without database
- Could swap to SQL Server, Postgres, etc. later

**Impact:**
- All data access through repositories
- MongoDB details never leak into handlers
- Testable with mocked repositories

### 4. DTO Layer (Not Anemic Models)

**Decision:** Models and DTOs are separate; mappers convert between them.

**Rationale:**
- Models encapsulate domain logic (methods, validation)
- DTOs are serializable contracts (no methods, just data)
- API contracts don't break domain logic changes

**Impact:**
- Mappers are required for all I/O
- Extra files but clearer intent

## Testing Strategy by Layer

### Shared Layer Tests
- **Type:** Unit tests (no dependencies)
- **Location:** `tests/Unit.Tests/Validators/`, `tests/Unit.Tests/Abstractions/`
- **Coverage:** Validators, Result<T>, exceptions
- **Tools:** xUnit, FluentAssertions, no mocks needed

### Api Layer Tests
- **Type:** Unit tests (mocked repositories)
- **Location:** `tests/Unit.Tests/Handlers/`
- **Coverage:** Handler logic, validation chains, error cases
- **Tools:** xUnit, NSubstitute (mock IIssueRepository), FluentAssertions
- **Example:** Mock repo returns null, verify handler returns failure

### Integration Tests
- **Type:** End-to-end (real MongoDB via TestContainers)
- **Location:** `tests/Integration.Tests/`
- **Coverage:** Full data flow (handler → repo → MongoDB)
- **Tools:** xUnit, TestContainers, real MongoDB instance
- **Example:** Create issue in handler, verify it persists in MongoDB

### Web Layer Tests
- **Type:** Component tests (bUnit for Blazor components)
- **Location:** `tests/Blazor.Tests/`
- **Coverage:** Component rendering, event handling, parameter binding
- **Tools:** bUnit, FluentAssertions

## Common Patterns & Best Practices

### Pattern: Result<T> for Error Handling

```csharp
// Instead of throwing exceptions
public async Task<Result<IssueDto>> CreateIssue(CreateIssueCommand command)
{
  var validation = Validate(command);
  if (!validation.IsValid)
    return Result<IssueDto>.Fail("Validation failed");
  
  var issue = new Issue(...);
  var created = await _repository.CreateAsync(issue);
  
  if (!created)
    return Result<IssueDto>.Fail("Database error");
  
  return Result<IssueDto>.Ok(IssueMapper.ToDto(issue));
}

// Consumers check result
var result = await handler.Handle(command);
if (result.Success)
{
  return Ok(result.Value);
}
else
{
  return BadRequest(result.ErrorMessage);
}
```

**Benefits:** No exceptions for flow control, explicit error paths, easier testing.

### Pattern: Async All The Way Down

```csharp
// ✅ CORRECT
public async Task<bool> CreateAsync(Issue entity)
{
  await _collection.InsertOneAsync(entity);
  return true;
}

// ❌ WRONG (blocking)
public async Task<bool> CreateAsync(Issue entity)
{
  _collection.InsertOne(entity);
  return true;
}
```

### Pattern: Null-Coalescing in Mappers

```csharp
// ✅ EXPLICIT
ArchivedBy = model.ArchivedBy is not null 
  ? UserMapper.ToDto(model.ArchivedBy) 
  : UserDto.Empty,

// Map it back
ArchivedBy = dto.ArchivedBy != UserDto.Empty
  ? UserMapper.ToModel(dto.ArchivedBy)
  : null,
```

## References & Further Reading

- **Vertical Slice Architecture:** https://jimmybogard.com/vertical-slice-architecture/
- **CQRS Pattern:** https://martinfowler.com/bliki/CQRS.html
- **FluentValidation:** https://fluentvalidation.net/
- **NetArchTest:** https://github.com/BenMorris/NetArchTest
- **MongoDB.EntityFrameworkCore:** https://www.mongodb.com/docs/entity-framework/current/
- **Aspire:** https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview
- **Blazor SSR:** https://learn.microsoft.com/en-us/aspnet/core/blazor/

## Confidence Justification

**`low`** because:
- ✅ Patterns are explicitly implemented and tested (NetArchTest enforces them)
- ✅ Project structure is consistent (vertical slices follow same pattern)
- ⏳ **Waiting for:** Frodo to use this skill in documenting new architecture changes
- ⏳ **Waiting for:** Multiple developers to validate patterns match their understanding
- ⏳ **Waiting for:** Cross-team use to discover gaps or missing details

Will bump to `medium` after Frodo documents 2-3 architecture changes using this skill.  
Will bump to `high` after team validates patterns in code reviews and confirms docs accuracy.
