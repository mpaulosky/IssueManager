# Decision: API Versioning Strategy

**Date:** 2026-02-28  
**Author:** Sam (Backend Developer)  
**Status:** Implemented  
**Scope:** Api project

## Context

Added formal API versioning infrastructure using Asp.Versioning.Http package (v8.1.0). The existing endpoints already use `/api/v1/` route prefixes, but there was no formal versioning framework configured.

## Decision

Implemented `ApiVersioningExtensions.AddApiVersioning()` with the following configuration:

- **Default version:** 1.0
- **AssumeDefaultVersionWhenUnspecified:** true (gracefully handle unversioned requests)
- **ReportApiVersions:** true (expose supported versions in response headers)
- **Multiple version readers:**
  - URL segment (primary) — already in use via `/api/v1/` patterns
  - `X-Api-Version` header
  - `api-version` query string parameter

## Rationale

1. **Minimal disruption:** Existing `/api/v1/` route patterns work as-is; no endpoint changes needed
2. **Flexibility:** Clients can specify version via URL, header, or query string
3. **Graceful defaults:** Unversioned requests automatically use v1.0
4. **Transparency:** Response headers advertise available API versions

## Implementation

- Package: `Asp.Versioning.Http` 8.1.0 in `Directory.Packages.props`
- Extension: `src/Api/Extensions/ApiVersioningExtensions.cs`
- Registration: `builder.AddApiVersioning()` in `Program.cs` after Auth0 setup
- Did **not** add `AddApiExplorer()` — simplified for initial implementation

## Future Considerations

- When adding v2 endpoints, consider enabling `AddApiExplorer()` for enhanced OpenAPI/Swagger support
- Document version deprecation policy when rolling out new API versions
- Consider adding `ApiVersion` attributes to endpoint groups once we have multiple versions

## Related Files

- `Directory.Packages.props` (package version)
- `src/Api/Api.csproj` (package reference)
- `src/Api/Extensions/ApiVersioningExtensions.cs` (extension method)
- `src/Api/Program.cs` (registration)
