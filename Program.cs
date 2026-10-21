using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using EventEase;
using EventEase.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// App state: singletons so every component shares one source of truth.
builder.Services.AddSingleton<BrowserStorage>();
builder.Services.AddSingleton<EventService>();
builder.Services.AddSingleton<RegistrationService>();
builder.Services.AddSingleton<SessionState>();

var host = builder.Build();

// Hydrate persisted state once, before the first render, so pages never see empty/partial state.
await host.Services.GetRequiredService<EventService>().InitializeAsync();
await host.Services.GetRequiredService<RegistrationService>().InitializeAsync();
await host.Services.GetRequiredService<SessionState>().InitializeAsync();

await host.RunAsync();
