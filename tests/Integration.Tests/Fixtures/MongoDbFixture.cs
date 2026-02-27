namespace Integration.Fixtures;

/// <summary>
/// Shared MongoDB TestContainer fixture for integration tests.
/// </summary>
public class MongoDbFixture : IAsyncLifetime
{
	private const string MongodbImage = "mongo:8.2";
	private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder()
			.WithImage(MongodbImage)
			.Build();

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
