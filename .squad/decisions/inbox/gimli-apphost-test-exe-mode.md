### 2026-03-07: AppHost.Tests.Unit uses direct executable mode in CI

**By:** Gimli
**What:** AppHost.Tests.Unit (OutputType=Exe) runs as direct executable in CI, consistent with
Architecture.Tests and Web.Tests.Bunit. Not via dotnet test.
**Why:** xUnit v3 TestProcessLauncherAdapter compatibility issue causes inconclusive tests when
dotnet test is used with OutputType=Exe projects.
