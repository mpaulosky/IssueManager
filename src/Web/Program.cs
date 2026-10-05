// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     Program.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Web
// =======================================================

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddAuth0();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<TokenForwardingHandler>();

builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

builder.Services.AddOutputCache();
builder.Services.AddRadzenComponents();
builder.Services.AddBlazoredLocalStorage();

// Service discovery resolves the API by its AppHost resource name.
var apiBaseAddress = new Uri($"https+http://{Shared.Constants.Constants.ApiService}");

builder.Services.AddHttpClient<IIssueApiClient, IssueApiClient>(client =>
	client.BaseAddress = apiBaseAddress)
	.AddServiceDiscovery()
	.AddHttpMessageHandler<TokenForwardingHandler>();

builder.Services.AddHttpClient<ICategoryApiClient, CategoryApiClient>(client =>
	client.BaseAddress = apiBaseAddress)
	.AddServiceDiscovery()
	.AddHttpMessageHandler<TokenForwardingHandler>();

builder.Services.AddHttpClient<IStatusApiClient, StatusApiClient>(client =>
	client.BaseAddress = apiBaseAddress)
	.AddServiceDiscovery()
	.AddHttpMessageHandler<TokenForwardingHandler>();

builder.Services.AddHttpClient<ICommentApiClient, CommentApiClient>(client =>
	client.BaseAddress = apiBaseAddress)
	.AddServiceDiscovery()
	.AddHttpMessageHandler<TokenForwardingHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseOutputCache();

app.MapGet("/auth/login", async Task (HttpContext httpContext, string returnUrl = "/") =>
{
	// Only redirect back into this app; a crafted returnUrl must not send the user off-site.
	var redirectUri = Microsoft.AspNetCore.Http.HttpResults.RedirectHttpResult.IsLocalUrl(returnUrl) ? returnUrl : "/";
	var authProperties = new LoginAuthenticationPropertiesBuilder()
		.WithRedirectUri(redirectUri)
		.Build();
	await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authProperties);
});

app.MapGet("/auth/logout", async httpContext =>
{
	var authProperties = new LogoutAuthenticationPropertiesBuilder()
		.WithRedirectUri("/")
		.Build();
	await httpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authProperties);
	await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
});

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
