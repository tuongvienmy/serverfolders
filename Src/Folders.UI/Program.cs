using Folders.UI;
using Folders.UI.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

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
