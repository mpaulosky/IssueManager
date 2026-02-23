### 2026-02-23: Release notes automation implemented
**By:** Aragorn (Backend Dev)
**What:** Created .github/release.yml (label→section mapping) and updated squad-release.yml (trigger on tag push, runs dotnet test, uses --generate-notes). Zero external dependencies — GitHub native feature.
**Why:** Matthew approved Gandalf's recommendation for GitHub native auto-generated release notes.
