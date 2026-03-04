// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ApiVersioningExtensions.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =============================================

using Asp.Versioning;

namespace Api.Extensions;

/// <summary>
/// Provides API versioning extension methods.
/// </summary>
public static class ApiVersioningExtensions
{
	/// <summary>
	/// Adds API versioning configured for IssueManager (default v1.0, URL segment style).
	/// </summary>
	public static WebApplicationBuilder AddApiVersioning(this WebApplicationBuilder builder)
	{
		builder.Services.AddApiVersioning(options =>
		{
			options.DefaultApiVersion = new ApiVersion(1, 0);
			options.AssumeDefaultVersionWhenUnspecified = true;
			options.ReportApiVersions = true;
			options.ApiVersionReader = ApiVersionReader.Combine(
				new UrlSegmentApiVersionReader(),
				new HeaderApiVersionReader("X-Api-Version"),
				new QueryStringApiVersionReader("api-version")
			);
		});

		return builder;
	}
}
