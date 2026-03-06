### 2026-03-07: Test Projects Use RootNamespace = Unit to Preserve Namespace Structure
**By:** Gimli (Tester) via Copilot
**What:** When `tests/Unit.Tests` was split into `Api.Tests.Unit`, `Shared.Tests.Unit`, and `Web.Tests.Unit`, all three new `.csproj` files were given `<RootNamespace>Unit</RootNamespace>`. This preserves the existing `Tests.Unit.*` file-level namespace declarations without renaming any test files.
**Why:** Renaming the namespace in 60+ test files would create a noisy diff with no functional benefit. Keeping `RootNamespace = Unit` is the standard IssueManager approach for split test assemblies that share a logical namespace.
**Scribe:** Merge into .squad/decisions.md
