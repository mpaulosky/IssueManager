### 2026-03-07: pre-push Hook Path Patterns Must NOT Include a Leading Slash
**By:** Boromir (DevOps) via Copilot
**What:** In `scripts/hooks/pre-push`, the `case` statement path patterns for project detection must NOT begin with `/`. The `find src tests` command returns relative paths (e.g., `src/Web/Components/Foo.cs`), not absolute paths. Patterns like `*"/src/Web/"*` will never match because there is no leading `/`.
**Correct pattern:**
```bash
case "$file" in
  *"src/Web/"*)  expected_project="Web" ;;
  *"src/Api/"*)  expected_project="Api" ;;
  ...
esac
```
**Wrong pattern (breaks silently):**
```bash
case "$file" in
  *"/src/Web/"*)  expected_project="Web" ;;   # never matches
esac
```
**Why:** This bug was introduced twice during the Unit.Tests split session. Each time the pre-push hook was edited, leading slashes were accidentally added and then had to be corrected before gates would pass.
**Scribe:** Merge into .squad/decisions.md
