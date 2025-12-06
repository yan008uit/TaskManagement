using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using TaskManagementWeb.Services;
using TaskManagementWeb.Components;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add HttpClient for API calls
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7290/api/")
});

// Add ApiClient
builder.Services.AddScoped<ApiClient>(sp =>
{
    var http = sp.GetRequiredService<HttpClient>();
    var js = sp.GetRequiredService<IJSRuntime>();
    return new ApiClient(http, js);
});

// Add frontend API services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProjectApiService>();
builder.Services.AddScoped<TaskApiService>();
builder.Services.AddScoped<CommentApiService>();

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddAuthorizationCore();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();