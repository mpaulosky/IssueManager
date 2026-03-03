# 2026-03-03: Created copilot-sdk-csharp-usage skill

**By:** Frodo

**What:** Created `.squad/skills/copilot-sdk-csharp-usage/SKILL.md` from removed instruction file

**Why:** Replaced `.github/instructions/copilot-sdk-csharp.instructions.md`. Instruction files load globally on every .cs file; skills are on-demand reference material. This reduces cognitive load for developers while preserving specialized SDK guidance for when IssueManager integrates Copilot features.

**Structure:**
- Overview: SDK purpose for .NET 10+ applications
- When to Use: IssueManager integration scenarios
- Installation: NuGet package + requirements
- Key Patterns: Client init, sessions, events, tools, BYOK, connectivity, lifecycle
- Best Practices: 10 focused guidelines
- Gotchas: 8 technical preview caveats
- References: Official docs and best practices

**Status:** Complete. Skill available at `.squad/skills/copilot-sdk-csharp-usage/SKILL.md`
