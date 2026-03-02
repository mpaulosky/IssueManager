// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ApiWebApplicationFactory.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Unit Tests
// =======================================================

namespace Tests.Unit.Endpoints;

/// <summary>
/// WebApplicationFactory for endpoint tests. Replaces repository singletons with NSubstitute fakes.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
	public IIssueRepository IssueRepository { get; } = Substitute.For<IIssueRepository>();
	public ICategoryRepository CategoryRepository { get; } = Substitute.For<ICategoryRepository>();
	public ICommentRepository CommentRepository { get; } = Substitute.For<ICommentRepository>();
	public IStatusRepository StatusRepository { get; } = Substitute.For<IStatusRepository>();

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.UseEnvironment("Testing");

		builder.ConfigureTestServices(services =>
		{
			// Replace real repository singletons with substitutes
			ReplaceService<IIssueRepository>(services, IssueRepository);
			ReplaceService<ICategoryRepository>(services, CategoryRepository);
			ReplaceService<ICommentRepository>(services, CommentRepository);
			ReplaceService<IStatusRepository>(services, StatusRepository);
		});
	}

	private static void ReplaceService<T>(IServiceCollection services, T implementation) where T : class
	{
		var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(T));
		if (descriptor is not null)
			services.Remove(descriptor);
		services.AddSingleton(implementation);
	}
}
