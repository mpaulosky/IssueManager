namespace IssueManager.ServiceDefaults;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// Extension methods for service defaults configuration.
/// </summary>
public static class Extensions
{
	/// <summary>
	/// Adds default service configuration for Aspire services.
	/// </summary>
	public static IServiceCollection AddServiceDefaults(this IServiceCollection services)
	{
		services.AddServiceDiscovery();
		return services;
	}

	/// <summary>
	/// Adds default health check configuration.
	/// </summary>
	public static IHealthChecksBuilder AddDefaultHealthChecks(this IServiceCollection services)
	{
		return services.AddHealthChecks();
	}
}
