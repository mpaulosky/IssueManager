// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CurrentUserExtensions.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

using Api.Services;

namespace Api.Extensions;

/// <summary>
/// Extension methods for registering current user services.
/// </summary>
public static class CurrentUserExtensions
{
	/// <summary>
	/// Registers the current user service and HTTP context accessor.
	/// </summary>
	public static IServiceCollection AddCurrentUser(this IServiceCollection services)
	{
		services.AddHttpContextAccessor();
		services.AddScoped<ICurrentUserService, CurrentUserService>();
		return services;
	}
}
