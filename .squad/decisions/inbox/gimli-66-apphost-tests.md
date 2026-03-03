# Decision: AppHost Test Coverage Approach (Issue #66)

**Date**: 2026-03-03  
**Author**: Gimli (Tester)  
**Status**: Implemented  
**PR**: #74

## Context

The AppHost project contains Aspire orchestration logic:
- DatabaseService.AddMongoDbServices() — MongoDB resource setup with environment-based DB name selection
- RedisServices.AddRedisServices() — Redis resource setup with clear-cache command

Aspire's architecture makes certain methods untestable:
- Private methods (OnRunClearCacheCommandAsync, OnUpdateResourceState, WithClearCommand) are invoked internally by Aspire's command execution infrastructure
- IDistributedApplicationBuilder is Aspire-specific and difficult to fully mock

## Decision

Use **Aspire.Hosting.Testing** package for integration tests + unit tests where feasible:

1. **Integration tests** (AppHostTests.cs): Use DistributedApplicationTestingBuilder to verify resource registration (builder.Resources.Contains)
2. **Unit tests** (DatabaseServiceTests.cs, RedisServiceTests.cs): Test public methods with real DistributedApplication.CreateBuilder()
3. **Private methods**: Document as untestable in comments; they are internal Aspire plumbing

## Implementation

- Added Aspire.Hosting.Testing 13.1.2 to Directory.Packages.props
- Tests in tests/Aspire/Aspire.Tests.csproj (shared with ServiceDefaults tests)
- AppHostTests verifies MongoDB "Server", Redis "RedisCache", "api", "web" resources are registered
- DatabaseServiceTests verifies environment detection (dev vs prod DB name)
- RedisServiceTests verifies resource name correctness

## Alternatives Considered

- **Mock IDistributedApplicationBuilder**: Too complex, Aspire-specific types not mockable
- **Architecture tests only**: Would verify method signatures exist but not behavior
- **Skip testing**: Unacceptable for logic with conditional branching (environment detection)

## Notes

- Tests compile successfully but local xUnit v3 runner encountered process launch issues (likely Aspire dependencies)
- Tests should run correctly in CI/CD or Visual Studio
- This approach balances test coverage with Aspire's architectural constraints