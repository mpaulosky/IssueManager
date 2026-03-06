### 2026-03-07: Blazor @ref Fields Must Not Be readonly — IDE0044 Suppressed
**By:** Gimli (Tester) via Copilot
**What:** Roslyn analyzer `IDE0044` ("Make field readonly") was suppressed in `.editorconfig` for `src/**/*.razor.cs` files. Blazor `@ref` fields (e.g., `private DataGrid _grid;`) cannot be `readonly` because Razor sets them at render time. The `readonly` keyword causes a compile error.
**Rule added to .editorconfig:**
```
[src/**/*.razor.cs]
dotnet_diagnostic.IDE0044.severity = none
```
**Why:** IDE0044 correctly flags these fields as "could be readonly" based on C# static analysis, but Blazor's component model requires them to remain mutable. Suppressing at the glob pattern level avoids per-field `[SuppressMessage]` attributes across every code-behind file.
**Scribe:** Merge into .squad/decisions.md
