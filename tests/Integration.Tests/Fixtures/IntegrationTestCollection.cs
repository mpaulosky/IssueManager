namespace Tests.Integration.Fixtures;

/// <summary>
/// Defines an xUnit collection that serializes all integration tests to prevent
/// Docker resource contention when multiple MongoDbContainers start simultaneously.
/// </summary>
[CollectionDefinition("Integration", DisableParallelization = true)]
public class IntegrationTestCollection
{
}
