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

### Documentation Standards

- **README.md is the first impression:** Must clearly identify the project (IssueManager), its purpose (issue management + modern architecture patterns), tech stack, and a quick-start path. Avoid placeholder or off-topic content.
- **SECURITY.md is mandatory:** Every public repository needs clear vulnerability reporting guidance. Include contact path, response timeline, supported versions, and security best practices. Don't reference unrelated projects.
- **Contributing path:** Link to `.ai-team/` for squad structure and `.github/CODE_OF_CONDUCT.md` for community guidelines. The squad model is our competitive advantage—surface it early.
- **Keep docs concise:** README ~200 words + getting started. Too long and contributors don't read it. Too vague and they're confused.
- **Consistency with code:** Documentation should reflect actual architecture (Vertical Slice, CQRS, MongoDB.EntityFramework, Aspire). Misalignment signals immaturity.

### Project Identity & Messaging

- IssueManager is a **reference architecture** for modern .NET applications—not just a tool. Highlight patterns, not just features.
- The squad governance model (Gandalf, Aragorn, Arwen, Gimli, Legolas, Galadriel, Elrond) is a differentiator. New contributors should know they're joining a well-structured team.
- Aspire is central to the story—local dev setup, orchestration, observability. It's not optional plumbing.

### Contributing Workflow Expectations

- Decisions are made through consensus and recorded in `.ai-team/decisions.md` (append-only ledger).
- Approval gates: Gandalf (architecture), Gimli (quality), Legolas (infrastructure).
- No merges without review. The squad model requires visibility and ownership.
- Team structure is defined in `.ai-team/routing.md`—contributors should understand domain boundaries and who to ask.
