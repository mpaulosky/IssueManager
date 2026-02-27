// Copyright (c) 2026. All rights reserved.

namespace Api.Extensions;

/// <summary>
/// Provides Auth0 JWT Bearer authentication extension methods for the API.
/// </summary>
public static class AuthExtensions
{
	/// <summary>
	/// Adds Auth0 JWT Bearer authentication to the API services.
	/// Only wires up authentication when AUTH0_DOMAIN and AUTH0_AUDIENCE configuration values are present.
	/// </summary>
	/// <remarks>
	/// Required configuration (set via user secrets or environment variables):
	/// <list type="bullet">
	///   <item><description>Auth0:Domain — your Auth0 tenant domain (e.g. your-tenant.auth0.com)</description></item>
	///   <item><description>Auth0:Audience — the API identifier registered in your Auth0 dashboard</description></item>
	/// </list>
	/// </remarks>
	public static WebApplicationBuilder AddAuth0(this WebApplicationBuilder builder)
	{
		var domain = builder.Configuration["Auth0:Domain"];
		var audience = builder.Configuration["Auth0:Audience"];

		if (string.IsNullOrEmpty(domain) || string.IsNullOrEmpty(audience))
		{
			// Auth0 is not yet configured — skip authentication setup.
			// Set Auth0:Domain and Auth0:Audience in user secrets or environment to enable.
			return builder;
		}

		builder.Services.AddAuthentication("Bearer")
			.AddJwtBearer("Bearer", options =>
			{
				options.Authority = $"https://{domain}/";
				options.Audience = audience;
				options.TokenValidationParameters = new()
				{
					ValidateIssuerSigningKey = true,
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
				};
			});

		builder.Services.AddAuthorization();

		return builder;
	}
}
