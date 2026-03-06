// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ServiceCollectionExtensionsTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =======================================================

namespace Unit.Extensions;

/// <summary>Unit tests for <see cref="ServiceCollectionExtensions"/>.</summary>
[ExcludeFromCodeCoverage]
public class ServiceCollectionExtensionsTests
{
	private static ServiceCollection CreateServices() => new();

	private static IConfiguration CreateConfiguration(string? connectionString = null)
	{
		var dict = new Dictionary<string, string?>();
		if (connectionString is not null)
			dict["ConnectionStrings:issuemanager"] = connectionString;

		return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
	}

	// ── AddRepositories ────────────────────────────────────────────────────

	[Fact]
	public void AddRepositories_RegistersIIssueRepository()
	{
		var services = CreateServices();
		services.AddRepositories(CreateConfiguration("mongodb://test:27017"));

		services.Should().Contain(sd => sd.ServiceType == typeof(IIssueRepository));
	}

	[Fact]
	public void AddRepositories_RegistersIStatusRepository()
	{
		var services = CreateServices();
		services.AddRepositories(CreateConfiguration("mongodb://test:27017"));

		services.Should().Contain(sd => sd.ServiceType == typeof(IStatusRepository));
	}

	[Fact]
	public void AddRepositories_RegistersICategoryRepository()
	{
		var services = CreateServices();
		services.AddRepositories(CreateConfiguration("mongodb://test:27017"));

		services.Should().Contain(sd => sd.ServiceType == typeof(ICategoryRepository));
	}

	[Fact]
	public void AddRepositories_RegistersICommentRepository()
	{
		var services = CreateServices();
		services.AddRepositories(CreateConfiguration("mongodb://test:27017"));

		services.Should().Contain(sd => sd.ServiceType == typeof(ICommentRepository));
	}

	[Fact]
	public void AddRepositories_UsesFallbackConnectionString_WhenNotConfigured()
	{
		var services = CreateServices();
		var act = () => services.AddRepositories(CreateConfiguration());

		act.Should().NotThrow();
		services.Should().Contain(sd => sd.ServiceType == typeof(IIssueRepository));
	}

	[Fact]
	public void AddRepositories_RegistersAsSingleton()
	{
		var services = CreateServices();
		services.AddRepositories(CreateConfiguration("mongodb://test:27017"));

		services.Where(sd => sd.ServiceType == typeof(IIssueRepository))
			.Should().AllSatisfy(sd => sd.Lifetime.Should().Be(ServiceLifetime.Singleton));
	}

	[Fact]
	public void AddRepositories_ReturnsSameServiceCollection()
	{
		var services = CreateServices();
		var result = services.AddRepositories(CreateConfiguration("mongodb://test:27017"));

		result.Should().BeSameAs(services);
	}

	// ── AddValidators ──────────────────────────────────────────────────────

	[Fact]
	public void AddValidators_RegistersAllValidators()
	{
		var services = CreateServices();
		services.AddValidators();

		services.Should().Contain(sd => sd.ServiceType == typeof(CreateIssueValidator));
		services.Should().Contain(sd => sd.ServiceType == typeof(UpdateIssueValidator));
		services.Should().Contain(sd => sd.ServiceType == typeof(DeleteIssueValidator));
		services.Should().Contain(sd => sd.ServiceType == typeof(ListIssuesQueryValidator));
		services.Should().Contain(sd => sd.ServiceType == typeof(UpdateIssueStatusValidator));
		services.Should().Contain(sd => sd.ServiceType == typeof(CreateStatusValidator));
		services.Should().Contain(sd => sd.ServiceType == typeof(UpdateStatusValidator));
		services.Should().Contain(sd => sd.ServiceType == typeof(CreateCategoryValidator));
		services.Should().Contain(sd => sd.ServiceType == typeof(UpdateCategoryValidator));
		services.Should().Contain(sd => sd.ServiceType == typeof(CreateCommentValidator));
		services.Should().Contain(sd => sd.ServiceType == typeof(UpdateCommentValidator));
		services.Should().Contain(sd => sd.ServiceType == typeof(DeleteCommentValidator));
	}

	[Fact]
	public void AddValidators_RegistersAsSingleton()
	{
		var services = CreateServices();
		services.AddValidators();

		services.Where(sd => sd.ServiceType == typeof(CreateIssueValidator))
			.Should().AllSatisfy(sd => sd.Lifetime.Should().Be(ServiceLifetime.Singleton));
	}

	[Fact]
	public void AddValidators_ReturnsSameServiceCollection()
	{
		var services = CreateServices();
		var result = services.AddValidators();

		result.Should().BeSameAs(services);
	}

	// ── AddHandlers ────────────────────────────────────────────────────────

	[Fact]
	public void AddHandlers_RegistersAllHandlers()
	{
		var services = CreateServices();
		services.AddHandlers();

		services.Should().Contain(sd => sd.ServiceType == typeof(CreateIssueHandler));
		services.Should().Contain(sd => sd.ServiceType == typeof(UpdateIssueHandler));
		services.Should().Contain(sd => sd.ServiceType == typeof(DeleteIssueHandler));
		services.Should().Contain(sd => sd.ServiceType == typeof(ListIssuesHandler));
		services.Should().Contain(sd => sd.ServiceType == typeof(GetIssueHandler));
		services.Should().Contain(sd => sd.ServiceType == typeof(UpdateIssueStatusHandler));
		services.Should().Contain(sd => sd.ServiceType == typeof(CreateStatusHandler));
		services.Should().Contain(sd => sd.ServiceType == typeof(GetStatusHandler));
		services.Should().Contain(sd => sd.ServiceType == typeof(ListStatusesHandler));
		services.Should().Contain(sd => sd.ServiceType == typeof(UpdateStatusHandler));
		services.Should().Contain(sd => sd.ServiceType == typeof(CreateCategoryHandler));
		services.Should().Contain(sd => sd.ServiceType == typeof(GetCategoryHandler));
		services.Should().Contain(sd => sd.ServiceType == typeof(ListCategoriesHandler));
		services.Should().Contain(sd => sd.ServiceType == typeof(UpdateCategoryHandler));
		services.Should().Contain(sd => sd.ServiceType == typeof(CreateCommentHandler));
		services.Should().Contain(sd => sd.ServiceType == typeof(GetCommentHandler));
		services.Should().Contain(sd => sd.ServiceType == typeof(ListCommentsHandler));
		services.Should().Contain(sd => sd.ServiceType == typeof(UpdateCommentHandler));
		services.Should().Contain(sd => sd.ServiceType == typeof(DeleteCommentHandler));
	}

	[Fact]
	public void AddHandlers_RegistersAsSingleton()
	{
		var services = CreateServices();
		services.AddHandlers();

		services.Where(sd => sd.ServiceType == typeof(CreateIssueHandler))
			.Should().AllSatisfy(sd => sd.Lifetime.Should().Be(ServiceLifetime.Singleton));
	}

	[Fact]
	public void AddHandlers_ReturnsSameServiceCollection()
	{
		var services = CreateServices();
		var result = services.AddHandlers();

		result.Should().BeSameAs(services);
	}
}
