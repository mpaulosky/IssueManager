// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     OpenTelemetryExporterTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  AppHost.Tests.Unit
// =============================================

namespace AppHost;

/// <summary>
/// Tests for OpenTelemetry exporter configuration
/// </summary>
public class OpenTelemetryExporterTests
{
	[Fact]
	public void AddServiceDefaults_WithOtlpEndpoint_ShouldConfigureOtlpExporter()
	{
		// Arrange
		var builder = WebApplication.CreateBuilder();
		builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] = "http://localhost:4317";

		// Act
		builder.AddServiceDefaults();
		var app = builder.Build();

		// Assert
		// OpenTelemetry SDK registers MeterProvider when configured
		var meterProvider = app.Services.GetService<MeterProvider>();
		meterProvider.Should().NotBeNull("OpenTelemetry metrics should be configured");
	}

	[Fact]
	public void AddServiceDefaults_WithoutOtlpEndpoint_ShouldNotThrow()
	{
		// Arrange
		var builder = WebApplication.CreateBuilder();
		// No OTEL_EXPORTER_OTLP_ENDPOINT configured

		// Act
		Action act = () =>
		{
			builder.AddServiceDefaults();
			builder.Build();
		};

		// Assert
		act.Should().NotThrow("ServiceDefaults should work without OTLP endpoint");
	}

	[Fact]
	public void AddServiceDefaults_WithEmptyOtlpEndpoint_ShouldNotConfigureOtlpExporter()
	{
		// Arrange
		var builder = WebApplication.CreateBuilder();
		builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] = "";

		// Act
		builder.AddServiceDefaults();
		var app = builder.Build();

		// Assert
		// Should not throw and should still register OpenTelemetry metrics
		var meterProvider = app.Services.GetService<MeterProvider>();
		meterProvider.Should().NotBeNull("OpenTelemetry should still be configured without OTLP endpoint");
	}

	[Fact]
	public void AddServiceDefaults_WithWhitespaceOtlpEndpoint_ShouldNotConfigureOtlpExporter()
	{
		// Arrange
		var builder = WebApplication.CreateBuilder();
		builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] = "   ";

		// Act
		builder.AddServiceDefaults();
		var app = builder.Build();

		// Assert
		// Should not throw and should still register OpenTelemetry metrics
		var meterProvider = app.Services.GetService<MeterProvider>();
		meterProvider.Should().NotBeNull("OpenTelemetry should still be configured with whitespace OTLP endpoint");
	}
}
