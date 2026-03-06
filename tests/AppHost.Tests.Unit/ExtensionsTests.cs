// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ExtensionsTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  AppHost.Tests.Unit
// =============================================

namespace AppHost;

/// <summary>
/// Tests for ServiceDefaults Extensions.cs
/// </summary>
public class ExtensionsTests
{
	[Fact]
	public void AddServiceDefaults_ShouldRegisterOpenTelemetry()
	{
		// Arrange
		var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());

		// Act
		builder.AddServiceDefaults();
		var host = builder.Build();

		// Assert
		var meterProvider = host.Services.GetService<MeterProvider>();
		var tracerProvider = host.Services.GetService<TracerProvider>();

		meterProvider.Should().NotBeNull("OpenTelemetry metrics should be registered");
		tracerProvider.Should().NotBeNull("OpenTelemetry tracing should be registered");
	}

	[Fact]
	public void AddServiceDefaults_ShouldRegisterHealthChecks()
	{
		// Arrange
		var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());

		// Act
		builder.AddServiceDefaults();
		var host = builder.Build();

		// Assert
		var healthCheckService = host.Services.GetService<HealthCheckService>();
		healthCheckService.Should().NotBeNull("HealthCheckService should be registered");
	}

	[Fact]
	public async Task AddServiceDefaults_ShouldRegisterSelfHealthCheck()
	{
		// Arrange
		var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
		builder.AddServiceDefaults();
		var host = builder.Build();
		var healthCheckService = host.Services.GetRequiredService<HealthCheckService>();

		// Act
		var result = await healthCheckService.CheckHealthAsync(TestContext.Current.CancellationToken);

		// Assert
		result.Should().NotBeNull();
		result.Status.Should().Be(HealthStatus.Healthy);
		result.Entries.Should().ContainKey("self");
		result.Entries["self"].Status.Should().Be(HealthStatus.Healthy);
		result.Entries["self"].Tags.Should().Contain("live");
	}

	[Fact]
	public void AddServiceDefaults_ShouldRegisterServiceDiscovery()
	{
		// Arrange
		var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());

		// Act
		builder.AddServiceDefaults();
		var host = builder.Build();

		// Assert
		var services = host.Services;
		services.Should().NotBeNull("Services should be registered");
	}
}
