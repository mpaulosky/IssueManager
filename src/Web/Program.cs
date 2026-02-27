using ServiceDefaults;
using Web;
using Web.Extensions;
using Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddAuth0();

builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

builder.Services.AddHttpClient<IIssueApiClient, IssueApiClient>(client =>
	client.BaseAddress = new Uri("https+http://api"))
	.AddServiceDiscovery();

builder.Services.AddHttpClient<ICategoryApiClient, CategoryApiClient>(client =>
	client.BaseAddress = new Uri("https+http://api"))
	.AddServiceDiscovery();

builder.Services.AddHttpClient<IStatusApiClient, StatusApiClient>(client =>
	client.BaseAddress = new Uri("https+http://api"))
	.AddServiceDiscovery();

builder.Services.AddHttpClient<ICommentApiClient, CommentApiClient>(client =>
	client.BaseAddress = new Uri("https+http://api"))
	.AddServiceDiscovery();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
