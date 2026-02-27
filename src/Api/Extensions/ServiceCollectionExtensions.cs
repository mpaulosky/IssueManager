// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ServiceCollectionExtensions.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api
// =======================================================

using Api.Handlers;
using Api.Handlers.Categories;
using Api.Handlers.Comments;
using Api.Handlers.Issues;
using Api.Handlers.Statuses;

namespace Api.Extensions;

/// <summary>Extension methods for registering application services.</summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers all MongoDB repositories, reading the connection string from configuration.
	/// Falls back to <c>mongodb://localhost:27017</c> if no connection string is configured.
	/// </summary>
	public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("issuemanager")
			?? "mongodb://localhost:27017";

		services.AddSingleton<IIssueRepository>(sp =>
			new IssueRepository(connectionString, "IssueManagerDb"));
		services.AddSingleton<IStatusRepository>(sp =>
			new StatusRepository(connectionString, "IssueManagerDb"));
		services.AddSingleton<ICategoryRepository>(sp =>
			new CategoryRepository(connectionString, "IssueManagerDb"));
		services.AddSingleton<ICommentRepository>(sp =>
			new CommentRepository(connectionString, "IssueManagerDb"));

		return services;
	}

	/// <summary>Registers all FluentValidation validators as singletons.</summary>
	public static IServiceCollection AddValidators(this IServiceCollection services)
	{
		services.AddSingleton<CreateIssueValidator>();
		services.AddSingleton<UpdateIssueValidator>();
		services.AddSingleton<DeleteIssueValidator>();
		services.AddSingleton<ListIssuesQueryValidator>();
		services.AddSingleton<UpdateIssueStatusValidator>();
		services.AddSingleton<CreateStatusValidator>();
		services.AddSingleton<UpdateStatusValidator>();
		services.AddSingleton<DeleteStatusValidator>();
		services.AddSingleton<CreateCategoryValidator>();
		services.AddSingleton<UpdateCategoryValidator>();
		services.AddSingleton<DeleteCategoryValidator>();
		services.AddSingleton<CreateCommentValidator>();
		services.AddSingleton<UpdateCommentValidator>();
		services.AddSingleton<DeleteCommentValidator>();

		return services;
	}

	/// <summary>Registers all CQRS handler classes as singletons.</summary>
	public static IServiceCollection AddHandlers(this IServiceCollection services)
	{
		services.AddSingleton<CreateIssueHandler>();
		services.AddSingleton<UpdateIssueHandler>();
		services.AddSingleton<DeleteIssueHandler>();
		services.AddSingleton<ListIssuesHandler>();
		services.AddSingleton<GetIssueHandler>();
		services.AddSingleton<UpdateIssueStatusHandler>();
		services.AddSingleton<CreateStatusHandler>();
		services.AddSingleton<GetStatusHandler>();
		services.AddSingleton<ListStatusesHandler>();
		services.AddSingleton<UpdateStatusHandler>();
		services.AddSingleton<DeleteStatusHandler>();
		services.AddSingleton<CreateCategoryHandler>();
		services.AddSingleton<GetCategoryHandler>();
		services.AddSingleton<ListCategoriesHandler>();
		services.AddSingleton<UpdateCategoryHandler>();
		services.AddSingleton<DeleteCategoryHandler>();
		services.AddSingleton<CreateCommentHandler>();
		services.AddSingleton<GetCommentHandler>();
		services.AddSingleton<ListCommentsHandler>();
		services.AddSingleton<UpdateCommentHandler>();
		services.AddSingleton<DeleteCommentHandler>();

		return services;
	}
}
