// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ICurrentUserService.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Services;

/// <summary>
/// Provides access to the currently authenticated user's identity.
/// </summary>
public interface ICurrentUserService
{
	/// <summary>
	/// Gets the Auth0 subject identifier (the user's unique ID) or null if not authenticated.
	/// </summary>
	string? UserId { get; }

	/// <summary>
	/// Gets the user's display name or null if not available.
	/// </summary>
	string? Name { get; }

	/// <summary>
	/// Gets the user's email address or null if not available.
	/// </summary>
	string? Email { get; }

	/// <summary>
	/// Gets a value indicating whether the current request is authenticated.
	/// </summary>
	bool IsAuthenticated { get; }
}
