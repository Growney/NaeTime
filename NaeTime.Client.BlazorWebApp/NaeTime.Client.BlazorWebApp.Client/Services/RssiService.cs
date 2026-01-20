using Microsoft.AspNetCore.SignalR.Client;
using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Client.BlazorWebApp.Client.Services;

public class RssiService : IAsyncDisposable
{
    public event Action<RssiValue>? OnRssiReceived;

    public HubConnection Connection { get; }
    public RssiService(string baseAddress)
    {
        string url = $"{baseAddress}/rssiHub";
        Connection = new HubConnectionBuilder()
            .WithUrl(url)
            .WithAutomaticReconnect()
            .Build();

        Connection.On<RssiValue>("ReceiveRssi", (rssiValue) =>
        {
            Console.WriteLine($"Rssi received: TimerId={rssiValue.TimerId}, Lane={rssiValue.Lane}, Rssi={rssiValue.Rssi}, SoftwareTime={rssiValue.SoftwareTime}, HardwareTime={rssiValue.HardwareTime}");
            OnRssiReceived?.Invoke(rssiValue);
        });
    }

    public Task StartAsync()
    {
        if(Connection.State == HubConnectionState.Connected)
        {
            return Task.CompletedTask;
        }

        return Connection.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await Connection.StopAsync();
        await Connection.DisposeAsync();
    }
}
