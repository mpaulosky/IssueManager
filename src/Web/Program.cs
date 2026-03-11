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

builder.Services.AddRadzenComponents();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddHttpClient<IIssueApiClient, IssueApiClient>(client =>
	client.BaseAddress = new Uri("https+http://api"))
	.AddServiceDiscovery()
	.AddHttpMessageHandler<TokenForwardingHandler>();

builder.Services.AddHttpClient<ICategoryApiClient, CategoryApiClient>(client =>
	client.BaseAddress = new Uri("https+http://api"))
	.AddServiceDiscovery()
	.AddHttpMessageHandler<TokenForwardingHandler>();

builder.Services.AddHttpClient<IStatusApiClient, StatusApiClient>(client =>
	client.BaseAddress = new Uri("https+http://api"))
	.AddServiceDiscovery()
	.AddHttpMessageHandler<TokenForwardingHandler>();

builder.Services.AddHttpClient<ICommentApiClient, CommentApiClient>(client =>
	client.BaseAddress = new Uri("https+http://api"))
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

app.MapGet("/auth/login", async void (HttpContext httpContext, string returnUrl = "/") =>
{
	var authProperties = new LoginAuthenticationPropertiesBuilder()
		.WithRedirectUri(returnUrl)
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
