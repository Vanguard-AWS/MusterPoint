using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MusterPoint.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add logging
builder.Services.AddLogging();

// Register services
// Use relative URLs - the host environment BaseAddress will handle routing
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuthStateProvider>();
builder.Services.AddScoped<BSdataService>();
builder.Services.AddScoped<DatasheetService>();
builder.Services.AddScoped<DetachmentService>();
builder.Services.AddScoped<ListStorageService>();
builder.Services.AddScoped<ListValidationService>();
builder.Services.AddScoped<ArmyRulesService>();

var host = builder.Build();

// Initialize auth state provider with AuthService
var authStateProvider = host.Services.GetRequiredService<AuthStateProvider>();
var authService = host.Services.GetRequiredService<AuthService>();
authService.SetAuthStateProvider(authStateProvider);

await host.RunAsync();
