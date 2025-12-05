using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddApiClients();
builder.Services.AddEventDbSignalRReactions(builder.HostEnvironment.BaseAddress);

await builder.Build().RunAsync();
