// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     Auth0AuthenticationStateProvider.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web
// =============================================

using System.Security.Claims;

namespace Web.Services;

public class Auth0AuthenticationStateProvider : AuthenticationStateProvider
{

	private readonly IHttpContextAccessor _httpContextAccessor;

	private readonly ILogger<Auth0AuthenticationStateProvider> _logger;

	public Auth0AuthenticationStateProvider(
			IHttpContextAccessor httpContextAccessor,
			ILogger<Auth0AuthenticationStateProvider> logger)
	{
		_httpContextAccessor = httpContextAccessor;
		_logger = logger;
	}

	/// <summary>
	/// The custom claim namespace used by Auth0 Actions to pass role information.
	/// Configure this in your Auth0 Post-Login Action to match.
	/// </summary>
	private const string RolesClaimNamespace = "https://articlesite.com/roles";

	public override Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		HttpContext? httpContext = _httpContextAccessor.HttpContext;

		if (httpContext?.User.Identity?.IsAuthenticated == true)
		{
			ClaimsPrincipal user = httpContext.User;

			// Log user claims for debugging
			_logger.LogDebug("User authenticated with {ClaimCount} claims", user.Claims.Count());

			foreach (Claim claim in user.Claims)
			{
				_logger.LogDebug("Claim: {Type} = {Value}", claim.Type, claim.Value);
			}

			// Create a new ClaimsIdentity with the existing claims plus any additional processing
			ClaimsIdentity identity = new(user.Identity);

			// Extract roles from the custom namespace claim (Auth0 Post-Login Action)
			ExtractRolesFromCustomNamespace(user, identity);

			// Check for Auth0 roles in the standard "roles" claim location
			ExtractRolesFromStandardClaim(user, identity);

			// Log the final roles for debugging
			var finalRoles = identity.Claims
				.Where(c => c.Type == ClaimTypes.Role)
				.Select(c => c.Value)
				.ToList();

			if (finalRoles.Count > 0)
			{
				_logger.LogInformation("User roles extracted: {Roles}", string.Join(", ", finalRoles));
			}
			else
			{
				_logger.LogWarning("No roles found for authenticated user. Check Auth0 Post-Login Action configuration.");
			}

			ClaimsPrincipal claimsPrincipal = new(identity);

			return Task.FromResult(new AuthenticationState(claimsPrincipal));
		}

		// Return an anonymous user if not authenticated
		ClaimsPrincipal anonymous = new(new ClaimsIdentity());

		return Task.FromResult(new AuthenticationState(anonymous));
	}

	/// <summary>
	/// Extracts roles from the custom namespace claim set by Auth0 Post-Login Action.
	/// Handles both comma-separated string format and JSON array format.
	/// </summary>
	private void ExtractRolesFromCustomNamespace(ClaimsPrincipal user, ClaimsIdentity identity)
	{
		// Find all claims with the custom namespace (Auth0 may send multiple claims for arrays)
		var namespaceClaims = user.FindAll(RolesClaimNamespace).ToList();

		foreach (Claim claim in namespaceClaims)
		{
			string value = claim.Value.Trim();

			if (string.IsNullOrEmpty(value))
			{
				continue;
			}

			// Handle JSON array format: ["Admin", "Author"]
			if (value.StartsWith('[') && value.EndsWith(']'))
			{
				try
				{
					var roles = System.Text.Json.JsonSerializer.Deserialize<string[]>(value);
					if (roles != null)
					{
						foreach (string role in roles)
						{
							AddRoleIfNotExists(identity, role);
						}
					}
				}
				catch (System.Text.Json.JsonException ex)
				{
					_logger.LogWarning(ex, "Failed to parse roles JSON array: {Value}", value);
				}
			}
			// Handle comma-separated string format: "Admin,Author"
			else if (value.Contains(','))
			{
				string[] roles = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
				foreach (string role in roles)
				{
					AddRoleIfNotExists(identity, role);
				}
			}
			// Handle single role value
			else
			{
				AddRoleIfNotExists(identity, value);
			}
		}
	}

	/// <summary>
	/// Extracts roles from the standard "roles" claim used by some Auth0 configurations.
	/// </summary>
	private void ExtractRolesFromStandardClaim(ClaimsPrincipal user, ClaimsIdentity identity)
	{
		IEnumerable<Claim> auth0Roles = user.FindAll("roles");

		foreach (Claim roleClaim in auth0Roles)
		{
			AddRoleIfNotExists(identity, roleClaim.Value);
		}
	}

	/// <summary>
	/// Adds a role claim if it doesn't already exist in the identity.
	/// </summary>
	private static void AddRoleIfNotExists(ClaimsIdentity identity, string role)
	{
		string trimmedRole = role.Trim();
		if (!string.IsNullOrEmpty(trimmedRole) && !identity.HasClaim(ClaimTypes.Role, trimmedRole))
		{
			identity.AddClaim(new Claim(ClaimTypes.Role, trimmedRole));
		}
	}

}
