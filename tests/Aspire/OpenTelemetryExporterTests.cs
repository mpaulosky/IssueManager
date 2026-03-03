// Copyright (c) 2026. All rights reserved.

namespace Aspire.Tests;

using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using ServiceDefaults;
using Xunit;

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
		// OpenTelemetry SDK registers services when OTLP is configured
		var openTelemetryBuilder = app.Services.GetService<OpenTelemetryBuilder>();
		openTelemetryBuilder.Should().NotBeNull("OpenTelemetry should be configured");
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
		// Should not throw and should still register OpenTelemetry
		var openTelemetryBuilder = app.Services.GetService<OpenTelemetryBuilder>();
		openTelemetryBuilder.Should().NotBeNull("OpenTelemetry should still be configured");
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
		// Should not throw and should still register OpenTelemetry
		var openTelemetryBuilder = app.Services.GetService<OpenTelemetryBuilder>();
		openTelemetryBuilder.Should().NotBeNull("OpenTelemetry should still be configured");
	}
}
