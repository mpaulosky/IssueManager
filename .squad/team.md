# IssueManager — Team Roster

> An issue management application built with .NET 10, Aspire, Blazor, MongoDB, CQRS, and Vertical Slice Architecture.

## Coordinator

| Name | Role | Notes |
|------|------|-------|
| Squad | Coordinator | Routes work, enforces handoffs and reviewer gates. Does not generate domain artifacts. |

## Members

| Name | Role | Charter | Status |
|------|------|---------|--------|
| Aragorn | Lead Developer | `.squad/agents/aragorn/charter.md` | ✅ Active |
| Gimli | Tester | `.squad/agents/gimli/charter.md` | ✅ Active |
| Scribe | Session Logger | `.squad/agents/scribe/charter.md` | 📋 Silent |
| Ralph | Work Monitor | — | 🔄 Monitor |

## Coding Agent

<!-- copilot-auto-assign: false -->

| Name | Role | Charter | Status |
|------|------|---------|--------|
| @copilot | Coding Agent | — | 🤖 Coding Agent |

### Capabilities

**🟢 Good fit — auto-route when enabled:**

- Bug fixes with clear reproduction steps
- Test coverage (adding missing tests, fixing flaky tests)
- Lint/format fixes and code style cleanup
- Dependency updates and version bumps
- Small isolated features with clear specs
- Boilerplate/scaffolding generation
- Documentation fixes and README updates

**🟡 Needs review — route to @copilot but flag for squad member PR review:**

- Medium features with clear specs and acceptance criteria
- Refactoring with existing test coverage
- API endpoint additions following established patterns
- Migration scripts with well-defined schemas

**🔴 Not suitable — route to squad member instead:**

- Architecture decisions and system design
- Multi-system integration requiring coordination
- Ambiguous requirements needing clarification
- Security-critical changes (auth, encryption, access control)
- Performance-critical paths requiring benchmarking
- Changes requiring cross-team discussion

## Issue Source

- **Repository:** mpaulosky/IssueManager
- **Connected:** 2026-02-24
- **Filters:** state:open

## Project Context

- **Owner:** Matthew Paulosky
- **Stack:** .NET 10, C# 14, Blazor (server-side), Aspire, MongoDB, CQRS, Vertical Slice Architecture, xUnit, FluentAssertions, NSubstitute, bUnit, TestContainers
- **Description:** An issue management application demonstrating vertical slice architecture, CQRS, and MongoDB integration in a production-ready .NET application.
- **Created:** 2026-02-24
