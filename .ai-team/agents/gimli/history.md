# History — Gimli

## Project Learnings (from init)

### IssueManager Project — Started 2026-02-17

**Tech Stack:**
- xUnit or NUnit for unit tests (TBD)
- Integration test patterns for CQRS handlers
- Mock/Stub strategies for MongoDB (use in-memory or testcontainers)

**Test strategy:**
- Coverage target: 80%+ for handlers, validators, and critical paths
- Unit tests for each Command/Query handler
- Integration tests for full vertical slices
- UI component tests for key Blazor components

**Edge cases to explore:**
- Handler failures (validation, domain rules)
- Concurrent operations (race conditions)
- Data state transitions (Issue lifecycle)
- API error responses

---

## Learnings

*Append test patterns, edge cases discovered, and quality insights here as you work.*
