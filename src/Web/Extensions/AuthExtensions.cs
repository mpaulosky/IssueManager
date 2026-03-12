// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     AuthExtensions.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web
// =============================================

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
	///   <item><description>Auth0: Domain — your Auth0 tenant domain (e.g., your-tenant.auth0.com)</description></item>
	///   <item><description>Auth0: ClientId — your Auth0 application's Client ID</description></item>
	///   <item><description>Auth0: ClientSecret — your Auth0 application's Client Secret</description></item>
	///   <item><description>Auth0: Audience — optional, the API identifier for token audience</description></item>
	/// </list>
	/// </remarks>
	/// <summary>
	/// The custom claim namespace used by Auth0 Actions to pass role information.
	/// This must match the namespace used in the Auth0 Post-Login Action.
	/// </summary>
	private const string RolesClaimNamespace = "https://articlesite.com/roles";

	public static WebApplicationBuilder AddAuth0(this WebApplicationBuilder builder)
	{
		var domain = builder.Configuration["Auth0:Domain"];
		var clientId = builder.Configuration["Auth0:ClientId"];

		if (string.IsNullOrEmpty(domain) || string.IsNullOrEmpty(clientId))
		{
			// Auth0 is not yet configured — skip authentication setup.
			// Set Auth0: Domain and Auth0: ClientId in user secrets or environment to enable.
			return builder;
		}

		builder.Services
			.AddAuth0WebAppAuthentication(options =>
			{
				options.Domain = domain;
				options.ClientId = clientId;
				options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
				options.Scope = "openid profile email";

				// Configure the OnTokenValidated event to extract custom claims
				options.OpenIdConnectEvents = new Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectEvents
				{
					OnTokenValidated = context =>
					{
						// Log all claims for debugging
						var logger = context.HttpContext.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("Auth0TokenValidation");
						logger?.LogInformation("Token validated for user, processing claims...");

						if (context.Principal?.Identity is System.Security.Claims.ClaimsIdentity identity)
						{
							// Log existing claims
							foreach (var claim in identity.Claims)
							{
								logger?.LogDebug("Token claim: {Type} = {Value}", claim.Type, claim.Value);
							}

							// Extract roles from custom namespace and add as standard role claims
							var rolesClaim = identity.FindFirst(RolesClaimNamespace);
							if (rolesClaim != null)
							{
								logger?.LogInformation("Found roles claim: {Value}", rolesClaim.Value);
								var value = rolesClaim.Value.Trim();

								// Handle JSON array format: ["Admin", "Author"]
								if (value.StartsWith('[') && value.EndsWith(']'))
								{
									try
									{
										var roles = System.Text.Json.JsonSerializer.Deserialize<string[]>(value);
										if (roles != null)
										{
											foreach (var role in roles)
											{
												if (!string.IsNullOrEmpty(role) && !identity.HasClaim(System.Security.Claims.ClaimTypes.Role, role))
												{
													identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role));
													logger?.LogInformation("Added role: {Role}", role);
												}
											}
										}
									}
									catch (System.Text.Json.JsonException ex)
									{
										logger?.LogWarning(ex, "Failed to parse roles JSON: {Value}", value);
									}
								}
								// Handle comma-separated string format
								else if (value.Contains(','))
								{
									var roles = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
									foreach (var role in roles)
									{
										var trimmed = role.Trim();
										if (!string.IsNullOrEmpty(trimmed) && !identity.HasClaim(System.Security.Claims.ClaimTypes.Role, trimmed))
										{
											identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, trimmed));
											logger?.LogInformation("Added role: {Role}", trimmed);
										}
									}
								}
								// Single role
								else if (!string.IsNullOrEmpty(value) && !identity.HasClaim(System.Security.Claims.ClaimTypes.Role, value))
								{
									identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, value));
									logger?.LogInformation("Added role: {Role}", value);
								}
							}
							else
							{
								logger?.LogWarning("No roles claim found at namespace: {Namespace}. Ensure Auth0 Post-Login Action is configured.", RolesClaimNamespace);
							}
						}

						return Task.CompletedTask;
					}
				};
			});

		builder.Services.AddCascadingAuthenticationState();
		builder.Services.AddAuthorization();

		return builder;
	}
}
