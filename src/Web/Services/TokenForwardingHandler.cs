// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     TokenForwardingHandler.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web
// =============================================

namespace Web.Services;

/// <summary>
/// A DelegatingHandler that forwards the Auth0 access token to outgoing API requests.
/// Attaches the Bearer token from the current user's authentication session.
/// </summary>
public sealed class TokenForwardingHandler : DelegatingHandler
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	/// <summary>
	/// Initializes a new instance of <see cref="TokenForwardingHandler"/>.
	/// </summary>
	public TokenForwardingHandler(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	/// <inheritdoc />
	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		var httpContext = _httpContextAccessor.HttpContext;
		if (httpContext is not null)
		{
			var accessToken = await httpContext.GetTokenAsync("access_token");
			if (!string.IsNullOrEmpty(accessToken))
			{
				request.Headers.Authorization =
					new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
			}
		}

		return await base.SendAsync(request, cancellationToken);
	}
}
