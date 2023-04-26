using Gpio;
using NaeTime.Node.Abstractions;
using NaeTime.Node.Abstractions.Enumeration;
using NaeTime.Node.Abstractions.Extensions;
using NaeTime.Node.Abstractions.Models;
using NaeTime.Node.Core;
using NaeTime.Node.Services;
using NaeTime.Shared;
using NaeTime.Shared.Node;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("Default").ConfigurePrimaryHttpMessageHandler(() =>

    new HttpClientHandler()
    {
        ClientCertificateOptions = ClientCertificateOption.Manual,
        ServerCertificateCustomValidationCallback =
            (httpRequestMessage, cert, cetChain, policyErrors) =>
            {
                return true;
            }
    }
);

builder.Services.AddGpioController();
builder.Services.AddStandardMcp3008();

builder.Services.AddSingleton<INodeTimeProvider, NodeTimeProvider>();

builder.Services.AddSingleton<RX5808ReceiverManager>();
builder.Services.AddSingleton<IRssiReceiverManager>(provider => provider.GetRequiredService<RX5808ReceiverManager>());
builder.Services.AddSingleton<IRX5808ReceiverManager>(provider => provider.GetRequiredService<RX5808ReceiverManager>());

builder.Services.AddSingleton<RssiPollingService>();
builder.Services.AddSingleton<IRssiPollingService>(provider => provider.GetRequiredService<RssiPollingService>());
builder.Services.AddHostedService(provider => provider.GetRequiredService<RssiPollingService>());

builder.Services.AddSingleton<RssiStreamSender>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<RssiStreamSender>());
builder.Services.AddSingleton<ICommunicationManager>(provider => provider.GetRequiredService<RssiStreamSender>());

builder.Services.AddSingleton<IRssiStreamAggregationQueue, RssiStreamAggregationQueue>();

builder.Services.AddSingleton<NodeConfigurationManager>();
builder.Services.AddSingleton<INodeConfigurationManager>(provider => provider.GetRequiredService<NodeConfigurationManager>());
builder.Services.AddHostedService(provider => provider.GetRequiredService<NodeConfigurationManager>());

var app = builder.Build();

app.MapPost("/config", async (HttpContext context) =>
{
    var logger = context.RequestServices.GetService<ILogger<Program>>();
    try
    {
        var configurationManager = context.RequestServices.GetRequiredService<INodeConfigurationManager>();
        var configurationDto = await context.Request.ReadFromJsonAsync<ConfigurationDto>();

        if (configurationDto != null)
        {
            var receivers = configurationDto.RX5808Receivers?.Select(x => new RX5808ReceiverConfiguration(x.Id, x.UseAnalogToDigitalConverter, x.RSSIPin, x.DataPin, x.SelectPin, x.ClockPin));
            if (receivers == null)
            {
                receivers = new List<RX5808ReceiverConfiguration>();
            }
            var configuration = new NodeConfiguration(configurationDto.Id, configurationDto.RssiTransmissionDelay, configurationDto.ServerUri, configurationDto.RssiRetryCount, receivers);

            await configurationManager.ApplyConfigurationAsync(configuration);
            logger?.LogInformation($"Node {configuration.NodeId} configuration applied");
            await configurationManager.StoreConfigurationAsync(configuration);
            logger?.LogInformation($"Node {configuration.NodeId} configuration stored");
            context.Response.StatusCode = StatusCodes.Status200OK;

            return Results.Ok();
        }
        else
        {
            return Results.BadRequest("Bad_config");
        }
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(ex.Message);
    }
});
app.MapGet("/config", async (HttpContext context) =>
{
    var configurationManager = context.RequestServices.GetRequiredService<INodeConfigurationManager>();

    if (configurationManager != null)
    {
        var config = await configurationManager.GetConfigurationAsync();
        if (config != null)
        {
            var configDto = new ConfigurationDto()
            {
                Id = config.NodeId,
                RssiRetryCount = config.RssiRetryCount,
                RssiTransmissionDelay = config.RssiTransmissionDelay,
                ServerUri = config.ServerUri,
            };
            var rx5808Receivers = new List<RX5808Dto>();
            foreach (var rx5808Config in config.RX5808Receivers)
            {
                rx5808Receivers.Add(new RX5808Dto()
                {
                    Id = rx5808Config.Id,
                    RSSIPin = rx5808Config.RSSIPin,
                    ClockPin = rx5808Config.ClockPin,
                    UseAnalogToDigitalConverter = rx5808Config.UseAnalogToDigitalConverter,
                    SelectPin = rx5808Config.ClockPin,
                    DataPin = rx5808Config.DataPin
                });
            }
            configDto.RX5808Receivers = rx5808Receivers;

            return Results.Ok(configDto);
        }
    }

    return Results.NoContent();
});
app.MapGet("/rssistream/{streamid}", (Guid streamId, HttpContext context) =>
{
    var receiverManagers = context.RequestServices.GetServices<IRssiReceiverManager>();
    foreach (var manager in receiverManagers)
    {
        if (manager.IsStreamManaged(streamId))
        {
            var streamInfo = manager.GetStream(streamId);
            if (streamInfo != null)
            {
                var dto = GetStreamDto(streamInfo);
                return Results.Ok(dto);
            }
        }
    }
    return Results.BadRequest("Stream_not_found");
});
app.MapPost("/rssistream/{streamid}/start", async (Guid streamId, RssiReceiverTypeDto? receiverType, int frequency, HttpContext context) =>
{
    try
    {
        var receiverManagers = context.RequestServices.GetServices<IRssiReceiverManager>();
        foreach (var manager in receiverManagers)
        {
            if ((receiverType == null || receiverType == GetReceiverType(manager.ReceiverType)) && manager.CanHandleNewStream(frequency))
            {
                var streamInfo = await manager.EnableStreamAsync(streamId, frequency);

                if (streamInfo != null)
                {
                    var dto = GetStreamDto(streamInfo);

                    return Results.Created($"/rssistream/{streamId}", dto);
                }

                break;
            }
        }
        return Results.BadRequest("No_Capable_Receiver_Found");
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});
app.MapPost("/rssistream/{streamid}/stop", (Guid streamId, HttpContext context) =>
{
    try
    {
        var receiverManagers = context.RequestServices.GetServices<IRssiReceiverManager>();
        foreach (var manager in receiverManagers)
        {
            if (manager.IsStreamManaged(streamId))
            {
                manager.DisableStream(streamId);
                return Results.Ok();
            }
        }
        return Results.BadRequest("Stream_not_found");
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});
app.MapGet("/receivers", (HttpContext context) =>
{
    var receiverManagers = context.RequestServices.GetServices<IRssiReceiverManager>();
    var receivers = new List<RssiReceiverDto>();

    foreach (var manager in receiverManagers)
    {
        foreach (var receiver in manager.GetReceivers())
        {
            receivers.Add(new RssiReceiverDto()
            {
                Id = receiver.Id,
                IsRssiStreamEnabled = receiver.IsStreamEnabled(),
                AssignedFrequency = receiver.AssignedFrequency,
                TunedFrequency = receiver.TunedFrequency,
                StreamId = receiver.CurrentStream?.Id ?? Guid.Empty,
            });
        }
    }

    return Results.Ok(receivers);
});

app.Run();

static RssiStreamDto GetStreamDto(RssiStream streamInfo)
{
    var streamDto = new RssiStreamDto()
    {
        Id = streamInfo.Id,
        ReceiverId = streamInfo.ReceiverId,
        ReceiverType = GetReceiverType(streamInfo.ReceiverType),
        StartTick = streamInfo.StartTick,
        AssignedFrequency = streamInfo.AssignedFrequency,
        TunedFrequency = streamInfo.TunedFrequency
    };
    return streamDto;
}
static RssiReceiverTypeDto GetReceiverType(RssiReceiverType receiverType)
    => receiverType switch
    {
        RssiReceiverType.RX5808 => RssiReceiverTypeDto.RX5808,
        _ => throw new Exception("Unknown_receiver_type")
    };