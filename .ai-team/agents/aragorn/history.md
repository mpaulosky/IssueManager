# History — Aragorn

## Project Learnings (from init)

### IssueManager Project — Started 2026-02-17

**Tech Stack:**
- C#, .NET 10.0
- Vertical Slice Architecture
- CQRS pattern
- MongoDB.EntityFramework
- ASP.NET Core APIs

**Backend patterns to follow:**
- One vertical slice per feature
- Commands handle state changes, Queries return data
- Handlers are thin orchestrators (logic in services)
- Validators ensure data integrity before command execution
- DTOs shield domain entities from the wire

**Domain modeling:**
- Issues are the core aggregate
- Think about Issue state transitions (New, Active, Resolved, Closed)
- Use domain events if needed (IssueClosed, IssueAssigned, etc.)

---

## Learnings

*Append new patterns, MongoDB insights, and backend decisions here as you work.*
