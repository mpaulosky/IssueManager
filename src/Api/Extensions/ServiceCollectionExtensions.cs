// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ServiceCollectionExtensions.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

namespace Api.Extensions;

/// <summary>Extension methods for registering application services.</summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers all MongoDB repositories, reading the connection string from configuration.
	/// Selects the database name based on the current environment: <see cref="Shared.Constants.Constants.DevDatabaseName"/>
	/// for Development, <see cref="Shared.Constants.Constants.DatabaseName"/> for all other environments.
	/// Falls back to <c>mongodb://localhost:27017</c> if no connection string is configured.
	/// </summary>
	public static IServiceCollection AddRepositories(
		this IServiceCollection services,
		IConfiguration configuration)
	{

		const string databaseName = DatabaseName;

		var connectionString = configuration.GetConnectionString(databaseName) ?? "mongodb://localhost:27017";

		services.AddSingleton<IIssueRepository>(_ =>
			new IssueRepository(connectionString, databaseName));
		services.AddSingleton<IStatusRepository>(_ =>
			new StatusRepository(connectionString, databaseName));
		services.AddSingleton<ICategoryRepository>(_ =>
			new CategoryRepository(connectionString, databaseName));
		services.AddSingleton<ICommentRepository>(_ =>
			new CommentRepository(connectionString, databaseName));

		services.AddTransient<DatabaseSeeder>();

		return services;
	}

	/// <summary>Registers all FluentValidation validators as singletons.</summary>
	public static IServiceCollection AddValidators(this IServiceCollection services)
	{
		services.AddSingleton<CreateIssueValidator>();
		services.AddSingleton<UpdateIssueValidator>();
		services.AddSingleton<ListIssuesQueryValidator>();
		services.AddSingleton<UpdateIssueStatusValidator>();
		services.AddSingleton<CreateCommentValidator>();
		services.AddSingleton<UpdateCommentValidator>();

		return services;
	}

	/// <summary>Registers all CQRS handler classes as scoped.</summary>
	public static IServiceCollection AddHandlers(this IServiceCollection services)
	{
		services.AddScoped<CreateIssueHandler>();
		services.AddScoped<UpdateIssueHandler>();
		services.AddScoped<ListIssuesHandler>();
		services.AddScoped<GetIssueHandler>();
		services.AddScoped<UpdateIssueStatusHandler>();
		services.AddScoped<CreateCommentHandler>();
		services.AddScoped<GetCommentHandler>();
		services.AddScoped<ListCommentsHandler>();
		services.AddScoped<UpdateCommentHandler>();

		services.AddScoped<TaxonomyCrudHandler<CategoryDto, CreateCategoryCommand, UpdateCategoryCommand>>(sp =>
			new TaxonomyCrudHandler<CategoryDto, CreateCategoryCommand, UpdateCategoryCommand>(
				sp.GetRequiredService<ICategoryRepository>(), CategoryTaxonomyAdapter.Instance));
		services.AddScoped<TaxonomyCrudHandler<StatusDto, CreateStatusCommand, UpdateStatusCommand>>(sp =>
			new TaxonomyCrudHandler<StatusDto, CreateStatusCommand, UpdateStatusCommand>(
				sp.GetRequiredService<IStatusRepository>(), StatusTaxonomyAdapter.Instance));

		services.AddScoped<DeleteHandler<IssueDto>>(sp =>
			new DeleteHandler<IssueDto>(sp.GetRequiredService<IIssueRepository>(), "Issue"));
		services.AddScoped<DeleteHandler<CategoryDto>>(sp =>
			new DeleteHandler<CategoryDto>(sp.GetRequiredService<ICategoryRepository>(), "Category"));
		services.AddScoped<DeleteHandler<StatusDto>>(sp =>
			new DeleteHandler<StatusDto>(sp.GetRequiredService<IStatusRepository>(), "Status"));
		services.AddScoped<DeleteHandler<CommentDto>>(sp =>
			new DeleteHandler<CommentDto>(sp.GetRequiredService<ICommentRepository>(), "Comment"));

		return services;
	}
}
