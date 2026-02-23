namespace IssueManager.Tests.Integration.Fixtures;

/// <summary>
/// Shared MongoDB TestContainer fixture for integration tests.
/// </summary>
public class MongoDbFixture : IAsyncLifetime
{
	private const string MONGODB_IMAGE = "mongo:8.0";
	private readonly MongoDbContainer _mongoContainer;

	public MongoDbFixture()
	{
		_mongoContainer = new MongoDbBuilder()
			.WithImage(MONGODB_IMAGE)
			.Build();
	}

	/// <summary>
	/// Gets the MongoDB connection string.
	/// </summary>
	public string ConnectionString => _mongoContainer.GetConnectionString();

	/// <summary>
	/// Initializes the MongoDB container.
	/// </summary>
	public async Task InitializeAsync()
	{
		await _mongoContainer.StartAsync();
	}

	/// <summary>
	/// Disposes the MongoDB container.
	/// </summary>
	public async Task DisposeAsync()
	{
		await _mongoContainer.StopAsync();
		await _mongoContainer.DisposeAsync();
	}
}
