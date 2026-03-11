// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     AuthenticationServiceExtensions.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web
// =============================================

namespace Web.Services;

/// <summary>
/// Extension methods for configuring authentication and authorization services.
/// </summary>
public static class AuthenticationServiceExtensions
{

	/// <summary>
	/// Adds and configures authentication, authorization, and CORS services.
	/// </summary>
	public static void AddAuthenticationAndAuthorization(
			this IServiceCollection services,
			IConfiguration configuration)
	{

		services.AddHttpContextAccessor();

		services.AddAuth0WebAppAuthentication(options =>
		{

			options.Domain = configuration["Auth0:Domain"] ??
											throw new InvalidOperationException("Auth0:Domain configuration is missing.");

			options.ClientId = configuration["Auth0:ClientId"] ??
												throw new InvalidOperationException("Auth0:ClientId configuration is missing.");

			options.Scope = "openid profile email";

		});

		// Configure authentication state services
		services.AddScoped<AuthenticationStateProvider, Auth0AuthenticationStateProvider>();

		services.AddCascadingAuthenticationState();

	}

}
