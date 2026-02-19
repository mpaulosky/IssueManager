using IssueManager.ServiceDefaults;
using IssueManager.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceDefaults();

builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

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

app.MapHealthChecks("/health");

app.Run();
