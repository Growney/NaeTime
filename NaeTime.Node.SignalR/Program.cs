using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting; // Requires NuGet package
using NaeTime.Node.Extensions;
using NaeTime.Node.WebAPI.Map;
using Tensor.Mapping.Abstractions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(
    services =>
    {
        services.AddFileStorageNodeRepositories();
        services.AddNodeManager();
        services.AddSignalRNode();
        services.AddMapper(x =>
        {
            x.AddMappingAssembly(typeof(NodeConfigurationMap).Assembly);
        });
    })
    .Build();

await host.RunAsync();