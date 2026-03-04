// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     Extensions.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  ServiceDefaults
// =======================================================
namespace ServiceDefaults;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

/// <summary>
/// Extension methods for service defaults configuration.
/// </summary>
public static class Extensions
{
	/// <summary>
	/// Adds default service configuration for Aspire services.
	/// </summary>
	public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
	{
		builder.ConfigureOpenTelemetry();
		builder.AddDefaultHealthChecks();
		builder.Services.AddServiceDiscovery();
		
		return builder;
	}

	/// <summary>
	/// Configures OpenTelemetry tracing and metrics.
	/// </summary>
	private static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
	{
		builder.Services.AddOpenTelemetry()
			.WithMetrics(metrics =>
			{
				metrics
					.AddAspNetCoreInstrumentation()
					.AddHttpClientInstrumentation()
					.AddRuntimeInstrumentation();
			})
			.WithTracing(tracing =>
			{
				tracing
					.AddAspNetCoreInstrumentation()
					.AddHttpClientInstrumentation();
			});

		builder.AddOpenTelemetryExporters();

		return builder;
	}

	/// <summary>
	/// Adds OpenTelemetry exporters (OTLP for Aspire Dashboard).
	/// </summary>
	private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
	{
		var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

		if (useOtlpExporter)
		{
			builder.Services.AddOpenTelemetry().UseOtlpExporter();
		}

		return builder;
	}

	/// <summary>
	/// Adds default health check configuration.
	/// </summary>
	private static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
	{
		builder.Services.AddHealthChecks()
			.AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

		return builder;
	}

	/// <summary>
	/// Maps default endpoints for health checks, liveness, and readiness probes.
	/// </summary>
	public static WebApplication MapDefaultEndpoints(this WebApplication app)
	{
		if (app.Environment.IsDevelopment())
		{
			// All health checks (detailed)
			app.MapHealthChecks("/health");

			// Liveness probe (must respond to stay alive)
			app.MapHealthChecks("/alive", new HealthCheckOptions
			{
				Predicate = r => r.Tags.Contains("live")
			});
		}

		return app;
	}
}
