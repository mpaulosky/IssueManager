# Gandalf — History

## Project Context
- **Project:** IssueManager
- **Stack:** .NET 10, C# 14, Blazor (Interactive Server Rendering), MongoDB, EF Core, CQRS/MediatR, Vertical Slice Architecture, .NET Aspire
- **User:** Matthew Paulosky
- **Repo:** mpaulosky/IssueManager
- **Auth:** Auth0 for Authentication and Authorization
- **Joined:** 2026-02-27

## Day-1 Context
IssueManager is a .NET 10 Blazor Server application backed by MongoDB. It uses CQRS with MediatR and a Vertical Slice Architecture. The team has been building out repositories, handlers, endpoints, and tests. Auth0 is the designated identity provider for both authentication and authorization.

Key security concerns to address from day one:
- Auth0 integration completeness (SDK, flows, RBAC)
- All API endpoints and Blazor pages must enforce authorization
- MongoDB query safety (NoSQL injection vectors)
- No secrets in source — User Secrets and Key Vault patterns required
- Antiforgery token enforcement confirmed in Program.cs

## Learnings
<!-- Gandalf appends learnings here after each session -->
