// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     AuthExtensions.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =============================================

namespace Api.Extensions;

/// <summary>
/// Provides Auth0 JWT Bearer authentication extension methods for the API.
/// </summary>
public static class AuthExtensions
{
	/// <summary>
	/// Adds Auth0 JWT Bearer authentication to the API services.
	/// Only wires up real authentication when Auth0: Domain and Auth0: Audience configuration values are present.
	/// When those values are absent, falls back to a no-op scheme only if the environment is "Testing"
	/// or if <c>Auth:EnableNoAuthForTesting=true</c> is set; otherwise throws.
	/// </summary>
	/// <remarks>
	/// Required configuration (set via user secrets or environment variables):
	/// <list type="bullet">
	///   <item><description>Auth0: Domain — your Auth0 tenant domain (e.g., your-tenant.auth0.com)</description></item>
	///   <item><description>Auth0: Audience — the API identifier registered in your Auth0 dashboard</description></item>
	/// </list>
	/// </remarks>
	public static WebApplicationBuilder AddAuth0(this WebApplicationBuilder builder)
	{
		var domain = builder.Configuration["Auth0:Domain"];
		var audience = builder.Configuration["Auth0:Audience"];

		// Always add authorization services (required by UseAuthorization middleware)
		builder.Services.AddAuthorization();

		if (string.IsNullOrEmpty(domain) || string.IsNullOrEmpty(audience))
		{
			var isTestingEnvironment = builder.Environment.IsEnvironment("Testing");
			var enableNoAuthForTesting = builder.Configuration["Auth:EnableNoAuthForTesting"];
			var noAuthEnabled = isTestingEnvironment ||
				string.Equals(enableNoAuthForTesting, "true", StringComparison.OrdinalIgnoreCase);

			if (noAuthEnabled)
			{
				// Auth0 is not configured — add a no-op authentication scheme only for safe testing environments.
				builder.Services.AddAuthentication("NoAuth")
					.AddScheme<NoAuthOptions, NoAuthHandler>("NoAuth", _ => { });
				return builder;
			}

			throw new InvalidOperationException(
				"Auth0 configuration is missing (Auth0:Domain and/or Auth0:Audience). " +
				"Configure Auth0 for this environment, or set 'Auth:EnableNoAuthForTesting=true' " +
				"only in safe testing environments to bypass authentication.");
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

		return builder;
	}
}

/// <summary>
/// No-op authentication handler options.
/// </summary>
public class NoAuthOptions : Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions
{
}

/// <summary>
/// No-op authentication handler that allows all requests through without authentication.
/// Used when Auth0 is not configured (e.g., in local development or testing).
/// </summary>
public class NoAuthHandler : Microsoft.AspNetCore.Authentication.AuthenticationHandler<NoAuthOptions>
{
	public NoAuthHandler(
		Microsoft.Extensions.Options.IOptionsMonitor<NoAuthOptions> options,
		ILoggerFactory logger,
		System.Text.Encodings.Web.UrlEncoder encoder)
		: base(options, logger, encoder)
	{
	}

	protected override Task<Microsoft.AspNetCore.Authentication.AuthenticateResult> HandleAuthenticateAsync()
	{
		// No authentication - always succeed
		var identity = new System.Security.Claims.ClaimsIdentity("NoAuth");
		var principal = new System.Security.Claims.ClaimsPrincipal(identity);
		var ticket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(principal, "NoAuth");
		return Task.FromResult(Microsoft.AspNetCore.Authentication.AuthenticateResult.Success(ticket));
	}
}
