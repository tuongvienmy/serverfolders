using Folders.UI;
using Folders.UI.services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"];

builder.Services.AddScoped<FoldersApiClient>(sp => {
    // Get the IJSRuntime from DI
    var js = sp.GetRequiredService<IJSRuntime>();

    // Create HttpClient with base address
    var http = new HttpClient { BaseAddress = new Uri(apiBaseUrl!) };

    // Construct your client with both HttpClient and IJSRuntime
    return new FoldersApiClient(http, js);
});

await builder.Build().RunAsync();
