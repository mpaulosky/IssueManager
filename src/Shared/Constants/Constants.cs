// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     Constants.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Shared
// =======================================================

namespace Shared.Constants;

/// <summary>
///   Contains application-wide constant values.
/// </summary>
public static class Constants
{

	/// <summary>
	///   The name of the admin-only authorization policy.
	/// </summary>
	public const string AdminPolicy = "AdminOnly";

	/// <summary>
	///   The name of the cache resource.
	/// </summary>
	public const string Cache = "Cache";

	/// <summary>
	///   The server resource name.
	/// </summary>
	public const string Server = "Server";

	/// <summary>
	///   The dev database name.
	/// </summary>
	public const string DevDatabaseName = "dev-issuemanagerdb";

	/// <summary>
	///   The database name.
	/// </summary>
	public const string DatabaseName = "issuemanagerdb";

	/// <summary>
	///   The name of the default CORS policy.
	/// </summary>
	public const string DefaultCorsPolicy = "DefaultPolicy";

	/// <summary>
	///   The name of the output cache resource.
	/// </summary>
	public const string OutputCache = "output-cache";

	/// <summary>
	///   The name of the Redis cache resource.
	/// </summary>
	public const string RedisCache = "RedisCache";

	/// <summary>
	///   The name of the web application resource.
	/// </summary>
	public const string Website = "WebApp";

	/// <summary>
	///   The name of the API resource.
	/// </summary>
	public const string ApiService = "api-service";

	/// <summary>
	///   The name of the issue cache.
	/// </summary>
	public const string IssueCacheName = "IssueData";

}
