---
applyTo: "**/*.cs"
---
## File Header

Every new C# file MUST begin with this exact copyright block:

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

**Project Name Mapping:**
- `src/Api/` → `Api`
- `src/Web/` → `Web`
- `src/Shared/` → `Shared`
- `src/AppHost/` → `AppHost`
- `src/ServiceDefaults/` → `ServiceDefaults`
- `tests/Api.Tests.Unit/` → `Api.Tests.Unit`
- `tests/Shared.Tests.Unit/` → `Shared.Tests.Unit`
- `tests/Web.Tests.Unit/` → `Web.Tests.Unit`
- `tests/Web.Tests.Bunit/` → `Web.Tests.Bunit`
- `tests/AppHost.Tests.Unit/` → `AppHost.Tests.Unit`
- `tests/Api.Tests.Integration/` → `Api.Tests.Integration`

Replace `{FileName}` with the actual file name (without extension, then add the extension).
Replace `{ProjectName}` with the MSBuild project name based on the directory path.

**Examples:**
- `src/Api/Handlers/Issues/CreateIssueHandler.cs` → Project Name is `Api`
- `tests/Api.Tests.Unit/Handlers/Issues/CreateIssueHandlerTests.cs` → Project Name is `Api.Tests.Unit`

**Exceptions:** Do NOT add headers to generated files (`*.g.cs`, `obj/`, `bin/`, or auto-generated `GlobalUsings.cs` with no production logic).
