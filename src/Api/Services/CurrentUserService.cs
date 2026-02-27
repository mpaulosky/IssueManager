// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CurrentUserService.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

using System.Security.Claims;

namespace Api.Services;

/// <summary>
/// Reads the current user's identity from the HTTP context claims principal.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	/// <summary>
	/// Initializes a new instance of <see cref="CurrentUserService"/>.
	/// </summary>
	public CurrentUserService(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	/// <inheritdoc />
	public string? UserId =>
		_httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
		?? _httpContextAccessor.HttpContext?.User.FindFirstValue("sub");

	/// <inheritdoc />
	public string? Name =>
		_httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name)
		?? _httpContextAccessor.HttpContext?.User.FindFirstValue("name");

	/// <inheritdoc />
	public string? Email =>
		_httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email)
		?? _httpContextAccessor.HttpContext?.User.FindFirstValue("email");

	/// <inheritdoc />
	public bool IsAuthenticated =>
		_httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
