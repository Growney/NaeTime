using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NaeTime.Client;
using NaeTime.Client.Abstractions.Services;
using NaeTime.Client.Services;
using Syncfusion.Blazor;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace NaeTime.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddHttpClient("NaeTime.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("NaeTime.ServerAPI"));
            builder.Services.AddScoped<ICommunicationService, CommunicationService>();
            builder.Services.AddSyncfusionBlazor();
            builder.Services.AddApiAuthorization();
            builder.Services.AddSpeechSynthesis();

            var host = builder.Build();
            await host.RunAsync();
        }
    }
}