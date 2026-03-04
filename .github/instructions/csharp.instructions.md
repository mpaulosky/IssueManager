---
applyTo: "**/*.cs,**/*.razor,**/*.razor.cs"
---
## File Header

Every new C# or Razor file MUST begin with this exact copyright block:

**For .cs files:**
```csharp
// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     {FileName}.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  {ProjectName}
// =============================================
```

**For .razor files:**
```razor
@* ============================================
   Copyright (c) 2026. All rights reserved.
   File Name :     {FileName}.razor
   Company :       mpaulosky
   Author :        Matthew Paulosky
   Solution Name : IssueManager
   Project Name :  {ProjectName}
   ============================================= *@
```

**Project Name Mapping:**
- `src/Api/` → `Api`
- `src/Web/` → `Web`
- `src/Shared/` → `Shared`
- `src/AppHost/` → `AppHost`
- `src/ServiceDefaults/` → `ServiceDefaults`
- `tests/Unit.Tests/` → `Unit.Tests`
- `tests/Integration.Tests/` → `Integration.Tests`
- `tests/Blazor.Tests/` → `Blazor.Tests`
- `tests/Aspire/` → `Aspire`

Replace `{FileName}` with the actual file name (without extension, then add the extension).
Replace `{ProjectName}` with the MSBuild project name based on the directory path.

**Examples:**
- `src/Api/Handlers/Issues/CreateIssueHandler.cs` → Project Name is `Api`
- `src/Web/Components/IssueForm.razor` → Project Name is `Web`
- `tests/Unit.Tests/DTOs/IssueDtoTests.cs` → Project Name is `Unit.Tests`

**Exceptions:** Do NOT add headers to generated files (`*.g.cs`, `obj/`, `bin/`, or auto-generated `GlobalUsings.cs` with no production logic).
