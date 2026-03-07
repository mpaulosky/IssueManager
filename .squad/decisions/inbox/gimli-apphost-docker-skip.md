### 2026-03-07: AppHost integration tests skip gracefully without Docker

**By:** Gimli
**What:** AppHostTests.cs tests use DistributedApplicationFixture.IsAvailable + throw
SkipException.ForSkip() to skip when Aspire/Docker initialization fails. This converts
environment failures to explicit skips.
**Why:** AppHostTests.cs depends on DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>()
which may fail without Docker. Explicit skips are better than inconclusive failures.
Note: SkipException.ForSkip(string) is the correct factory method in xUnit v3.2.2 —
SkipException constructor is private; Skip.If() static helper does not exist in v3.2.2.
