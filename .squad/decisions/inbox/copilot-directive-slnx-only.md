### 2026-02-19: Use .slnx format exclusively—no .sln conversions

**By:** mpaulosky (via Copilot)

**What:** Team must use .slnx (new solution file format for .NET 10.0) in all processes. No conversions to legacy .sln format.

**Why:** User preference—ensures consistency with .NET 10.0 standard and prevents tooling confusion.

**Implementation:**
- All build workflows target `IssueManager.slnx` (not any `.sln` equivalent)
- Agents should never auto-generate or suggest `.sln` files
- When agents see `.sln` in output, treat it as a tooling error to flag
- Documentation and scripts reference `.slnx` exclusively
