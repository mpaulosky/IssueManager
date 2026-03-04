// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     AuthExtensions.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web
// =============================================

using Auth0.AspNetCore.Authentication;

namespace Web.Extensions;

/// <summary>
/// Provides Auth0 OpenID Connect authentication extension methods for the Blazor web application.
/// </summary>
public static class AuthExtensions
{
	/// <summary>
	/// Adds Auth0 OIDC authentication to the Blazor web application.
	/// Only wires up authentication when AUTH0_DOMAIN and AUTH0_CLIENT_ID configuration values are present.
	/// </summary>
	/// <remarks>
	/// Required configuration (set via user secrets or environment variables):
	/// <list type="bullet">
	///   <item><description>Auth0:Domain — your Auth0 tenant domain (e.g. your-tenant.auth0.com)</description></item>
	///   <item><description>Auth0:ClientId — your Auth0 application's Client ID</description></item>
	///   <item><description>Auth0:ClientSecret — your Auth0 application's Client Secret</description></item>
	///   <item><description>Auth0:Audience — optional, the API identifier for token audience</description></item>
	/// </list>
	/// </remarks>
	public static WebApplicationBuilder AddAuth0(this WebApplicationBuilder builder)
	{
		var domain = builder.Configuration["Auth0:Domain"];
		var clientId = builder.Configuration["Auth0:ClientId"];

		if (string.IsNullOrEmpty(domain) || string.IsNullOrEmpty(clientId))
		{
			// Auth0 is not yet configured — skip authentication setup.
			// Set Auth0:Domain and Auth0:ClientId in user secrets or environment to enable.
			return builder;
		}

		builder.Services
			.AddAuth0WebAppAuthentication(options =>
			{
				options.Domain = domain;
				options.ClientId = clientId;
				options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
			});

		builder.Services.AddCascadingAuthenticationState();
		builder.Services.AddAuthorization();

		return builder;
	}
}
