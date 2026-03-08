// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     MongoDbFixture.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Integration
// =======================================================
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Integration.Fixtures;

/// <summary>
/// Shared MongoDB TestContainer fixture for integration tests.
/// </summary>
[ExcludeFromCodeCoverage]
public class MongoDbFixture : IAsyncLifetime
{
	private const string MongodbImage = "mongo:latest";
  private MongoDbContainer? _mongoContainer;

	/// <summary>
	/// Runs once before the first instance is created.
	/// Resolves the active Docker context endpoint and sets <c>DOCKER_HOST</c>
	/// so Testcontainers connects to the correct Docker daemon.
	/// </summary>
	static MongoDbFixture()
	{
		EnsureDockerHostConfigured();
	}

	/// <summary>
 /// True when the MongoDB container started successfully; false when Docker is unavailable.
	/// </summary>
	public bool IsAvailable { get; private set; }

	/// <summary>
	/// Human-readable reason why Docker / the container is unavailable, or <see langword="null"/> when available.
	/// </summary>
	public string? UnavailableReason { get; private set; }

	/// <summary>
	/// Gets the MongoDB connection string.
	/// </summary>
  public string ConnectionString =>
		_mongoContainer?.GetConnectionString()
		?? throw new InvalidOperationException("MongoDB container is not running. Call ThrowIfUnavailable() first.");

	/// <summary>
	/// Throws a <see cref="SkipException"/> when Docker is unavailable so xUnit skips
	/// all tests in the class rather than failing them.  Call this at the top of each
	/// test-class constructor that receives this fixture.
	/// </summary>
	public void ThrowIfUnavailable()
	{
		if (!IsAvailable)
			throw SkipException.ForSkip(
				UnavailableReason ?? "Docker is unavailable. Ensure Docker Desktop is running and try again.");
	}

	/// <summary>
	/// Initializes the MongoDB container.
	/// </summary>
	public async ValueTask InitializeAsync()
	{
   try
		{
			_mongoContainer = new MongoDbBuilder(MongodbImage).Build();
			await _mongoContainer.StartAsync();
			IsAvailable = true;
		}
		catch (Exception ex)
		{
			IsAvailable = false;
			UnavailableReason =
				$"MongoDB container unavailable – ensure Docker Desktop is running and fully started. " +
				$"({ex.GetBaseException().Message})";
		}
	}

	/// <summary>
	/// Disposes the MongoDB container.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
    if (_mongoContainer is not null)
		{
			await _mongoContainer.StopAsync();
			await _mongoContainer.DisposeAsync();
		}
	}

	/// <summary>
	/// Reads <c>~/.docker/config.json</c> to find the active context, then reads
	/// that context's <c>meta.json</c> to extract the Docker endpoint.
	/// Sets <c>DOCKER_HOST</c> so Testcontainers uses the correct named pipe or socket.
	/// No-ops when <c>DOCKER_HOST</c> is already set externally.
	/// </summary>
	private static void EnsureDockerHostConfigured()
	{
		if (Environment.GetEnvironmentVariable("DOCKER_HOST") is not null)
			return;

		try
		{
			var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			var configPath = Path.Combine(home, ".docker", "config.json");
			if (!File.Exists(configPath)) return;

			using var configDoc = JsonDocument.Parse(File.ReadAllText(configPath));
			if (!configDoc.RootElement.TryGetProperty("currentContext", out var ctxProp)) return;

			var contextName = ctxProp.GetString();
			if (string.IsNullOrEmpty(contextName) || contextName == "default") return;

			// Docker CLI names context directories using the lowercase hex SHA-256 of the context name.
			var hash = Convert.ToHexString(
				SHA256.HashData(Encoding.UTF8.GetBytes(contextName))).ToLowerInvariant();

			var metaPath = Path.Combine(home, ".docker", "contexts", "meta", hash, "meta.json");
			if (!File.Exists(metaPath)) return;

			using var metaDoc = JsonDocument.Parse(File.ReadAllText(metaPath));
			if (!metaDoc.RootElement.TryGetProperty("Endpoints", out var endpoints)) return;
			if (!endpoints.TryGetProperty("docker", out var dockerEp)) return;
			if (!dockerEp.TryGetProperty("Host", out var hostProp)) return;

			var host = hostProp.GetString();
				if (!string.IsNullOrEmpty(host))
				{
					// Docker context stores npipe:////./pipe/<name> (4-slash format used by the Docker CLI)
					// but Docker.DotNet / Testcontainers requires npipe://./pipe/<name> (2-slash URI format).
					// Normalise before setting DOCKER_HOST to avoid an ArgumentException on URI parsing.
					if (host.StartsWith("npipe:////", StringComparison.OrdinalIgnoreCase))
						host = "npipe://" + host["npipe:////".Length..];

					Environment.SetEnvironmentVariable("DOCKER_HOST", host);
				}
		}
		catch
		{
			// If context resolution fails, let Testcontainers run its own endpoint detection.
		}
	}
}
