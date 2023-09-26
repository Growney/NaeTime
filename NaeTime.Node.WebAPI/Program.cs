using NaeTime.Node.Extensions;
using NaeTime.Node.WebAPI.Map;
using Tensor.Mapping.Abstractions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddNodeManager();
builder.Services.AddMapper(x =>
{
    x.AddMappingAssembly(typeof(NodeConfigurationMap).Assembly);
});

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
