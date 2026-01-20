using EventDbLite.Reactions.SignalR.Server;
using Microsoft.AspNetCore.SignalR;
using NaeTime.Client.BlazorWebApp.Hubs;
using NaeTime.Hardware;
using NaeTime.Hardware.Abstractions;

namespace NaeTime.Client.BlazorWebApp.Services;

public class RssiDistributionService : IRssiConsumer
{
    private readonly IHubContext<RssiHub> _hubContext;

    public RssiDistributionService(IHubContext<RssiHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public void HandleRssi(RssiValue value)
    {
        _hubContext.Clients.All.SendAsync("ReceiveRssi", new NaeTime.Query.Abstractions.Models.RssiValue(value.TimerId,value.Lane,value.Rssi,value.SoftwareTime,value.HardwareTime));
    }
}
