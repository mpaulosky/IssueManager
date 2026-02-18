# History — Gandalf

## Project Learnings (from init)

### IssueManager Project — Started 2026-02-17

**Tech Stack:**
- C#, .NET 10.0
- Aspire (orchestration)
- Blazor (frontend)
- MongoDB.EntityFramework (data access)
- CQRS (architecture pattern)
- Vertical Slice Architecture (organization)

**Key decisions:**
- Organize features as vertical slices (one slice = one feature, end-to-end)
- Use CQRS for command/query separation
- MongoDB as primary data store
- Aspire manages service topology and local dev setup
- Blazor for web UI (server-side or WebAssembly, TBD)

**Team members:**
- Gandalf (Lead) — Architecture, decisions, ceremonies
- Aragorn (Backend) — CQRS handlers, vertical slices, MongoDB
- Arwen (Frontend) — Blazor UI, integration
- Gimli (Tester) — Quality, test strategy, edge cases
- Legolas (DevOps) — Aspire, MongoDB ops, CI/CD

---

## Learnings

*Append new learnings, patterns, and decisions here as you work.*
