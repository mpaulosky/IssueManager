# Work Routing

How to decide who handles what.

## Routing Table

| Work Type | Route To | Examples |
|-----------|----------|----------|
| Architecture, API design, scope decisions | Aragorn | CQRS handlers, vertical slice design, PR reviews, tech decisions |
| Backend implementation (handlers, validators, mappers) | Aragorn | CQRS handlers, FluentValidation validators, DTO mappers |
| MongoDB data access / repositories | Sam | Repository implementations, DbContext, entity config, data models |
| Blazor UI / frontend | Legolas | Blazor components, pages, Tailwind CSS, server-side rendering, bUnit |
| CI/CD, build workflows | Boromir | GitHub Actions, dotnet build/test scripts, release automation, Aspire |
| Documentation, README, XML docs | Frodo | README, docs/, CONTRIBUTING, Scalar/OpenAPI descriptions, XML comments |
| Code review | Aragorn | Review PRs, check quality, enforce patterns |
| Unit testing, integration testing | Gimli | xUnit tests, FluentAssertions, NSubstitute mocks, coverage audits |
| Test scaffolding & builders | Gimli | Builder classes, test fixtures, TestContainers MongoDB setup |
| Async issue work (bugs, tests, small features) | @copilot 🤖 | Well-defined tasks matching capability profile |
| Session logging | Scribe | Automatic — never needs routing |

## Issue Routing

| Label | Action | Who |
|-------|--------|-----|
| `squad` | Triage: analyze issue, evaluate @copilot fit, assign `squad:{member}` label | Aragorn (Lead) |
| `squad:aragorn` | Pick up issue and complete the work | Aragorn |
| `squad:legolas` | Pick up issue and complete the work | Legolas |
| `squad:sam` | Pick up issue and complete the work | Sam |
| `squad:gimli` | Pick up issue and complete the work | Gimli |
| `squad:boromir` | Pick up issue and complete the work | Boromir |
| `squad:frodo` | Pick up issue and complete the work | Frodo |
| `squad:copilot` | Assign to @copilot for autonomous work (if enabled) | @copilot 🤖 |

### How Issue Assignment Works

1. When a GitHub issue gets the `squad` label, **Aragorn** triages it — analyzing content, evaluating @copilot's capability profile, assigning the right `squad:{member}` label, and commenting with triage notes.
2. **@copilot evaluation:** Aragorn checks if the issue matches @copilot's capability profile (🟢 good fit / 🟡 needs review / 🔴 not suitable). If it's a good fit, may route to `squad:copilot` instead.
3. When a `squad:{member}` label is applied, that member picks up the issue in their next session.
4. When `squad:copilot` is applied and auto-assign is enabled, `@copilot` is assigned on the issue and picks it up autonomously.
5. Members can reassign by removing their label and adding another member's label.
6. The `squad` label is the "inbox" — untriaged issues waiting for Aragorn review.

### Lead Triage Guidance for @copilot

When triaging, Aragorn should ask:

1. **Is this well-defined?** Clear title, reproduction steps or acceptance criteria, bounded scope → likely 🟢
2. **Does it follow existing patterns?** Adding a test, fixing a known bug, updating a dependency → likely 🟢
3. **Does it need design judgment?** Architecture, API design, UX decisions → likely 🔴
4. **Is it security-sensitive?** Auth, encryption, access control → always 🔴
5. **Is it medium complexity with specs?** Feature with clear requirements, refactoring with tests → likely 🟡

## Rules

1. **Eager by default** — spawn all agents who could usefully start work, including anticipatory downstream work.
2. **Scribe always runs** after substantial work, always as `mode: "background"`. Never blocks.
3. **Quick facts → coordinator answers directly.** Don't spawn an agent for "what port does the server run on?"
4. **When two agents could handle it**, pick the one whose domain is the primary concern.
5. **"Team, ..." → fan-out.** Spawn all relevant agents in parallel as `mode: "background"`.
6. **Anticipate downstream work.** If a feature is being built, spawn Gimli to write test cases from requirements simultaneously.
7. **Issue-labeled work** — when a `squad:{member}` label is applied to an issue, route to that member. Aragorn handles all `squad` (base label) triage.
8. **@copilot routing** — when evaluating issues, check @copilot's capability profile in `team.md`. Route 🟢 good-fit tasks to `squad:copilot`. Flag 🟡 needs-review tasks for PR review. Keep 🔴 not-suitable tasks with squad members.
