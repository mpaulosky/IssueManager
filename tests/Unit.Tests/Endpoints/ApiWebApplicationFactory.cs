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
/// WebApplicationFactory for endpoint tests. Replaces repository singletons with NSubstitute fakes
/// and uses a <see cref="TestAuthHandler"/> that only authenticates requests carrying an
/// <c>Authorization</c> header, allowing tests to verify both authenticated and unauthenticated paths.
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
			// Override NoAuth with a TestAuthHandler that only succeeds when an
			// Authorization header is present, so unauthorized paths can be tested.
			services.AddAuthentication("TestAuth")
				.AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestAuth", _ => { });

			// Replace real repository singletons with substitutes
			ReplaceService<IIssueRepository>(services, IssueRepository);
			ReplaceService<ICategoryRepository>(services, CategoryRepository);
			ReplaceService<ICommentRepository>(services, CommentRepository);
			ReplaceService<IStatusRepository>(services, StatusRepository);
		});
	}

	/// <summary>
	/// Creates an <see cref="HttpClient"/> pre-configured with an <c>Authorization</c> header
	/// so that requests to protected endpoints are treated as authenticated.
	/// </summary>
	public HttpClient CreateAuthenticatedClient()
	{
		var client = CreateClient();
		client.DefaultRequestHeaders.Add("Authorization", "Bearer TestToken");
		return client;
	}

	private static void ReplaceService<T>(IServiceCollection services, T implementation) where T : class
	{
		var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(T));
		if (descriptor is not null)
			services.Remove(descriptor);
		services.AddSingleton(implementation);
	}
}

/// <summary>
/// Test-only authentication handler that grants a successful identity only when an
/// <c>Authorization</c> header is present in the request.  Requests without the header
/// are treated as unauthenticated (NoResult), causing protected endpoints to return 401.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
	public TestAuthHandler(
		IOptionsMonitor<AuthenticationSchemeOptions> options,
		ILoggerFactory logger,
		UrlEncoder encoder)
		: base(options, logger, encoder) { }

	protected override Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		if (StringValues.IsNullOrEmpty(Request.Headers.Authorization))
			return Task.FromResult(AuthenticateResult.NoResult());

		var identity = new ClaimsIdentity("TestAuth");
		var principal = new ClaimsPrincipal(identity);
		var ticket = new AuthenticationTicket(principal, "TestAuth");
		return Task.FromResult(AuthenticateResult.Success(ticket));
	}
}
