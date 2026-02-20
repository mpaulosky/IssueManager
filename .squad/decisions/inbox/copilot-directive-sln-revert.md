### 2026-02-20: Revert to .sln format for CI compatibility

**By:** mpaulosky (via Copilot)

**What:** CI/CD pipeline failing with .slnx format. Reverting all processes to use legacy .sln solution format. GitHub Actions workflows and build commands must reference .sln instead of .slnx.

**Why:** The squad-ci.yml build step is failing because `dotnet restore` and `dotnet build` do not properly support the modern .slnx format in the CI environment. GitHub Actions on ubuntu-latest cannot parse .slnx files correctly. Reverting to .sln ensures compatibility with existing CI infrastructure.
