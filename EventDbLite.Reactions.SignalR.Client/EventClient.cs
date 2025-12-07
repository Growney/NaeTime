
using EventDbLite.Abstractions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace EventDbLite.Reactions.SignalR.Client;
public class EventClient : IEventClient
{
    public event Func<StreamEvent, Task>? OnEventReceived;

    private readonly ILogger<EventClient> _logger;

    private readonly string _url;
    public HubConnection Connection { get; }

    public EventClient(ILogger<EventClient> logger, string baseAddress)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _url = $"{baseAddress}/eventDbLiteHub";

        _logger.LogInformation("Initializing EventClient with URL: {Url}", _url);

        Connection = new HubConnectionBuilder()
            .WithUrl(_url)
            .WithAutomaticReconnect()
            .Build();

        Connection.On<StreamEvent>("ReceiveEvent", async (streamEvent) =>
        {
            _logger.LogDebug("Event received: {Identifier} at {GlobalPosition}", streamEvent.Data.Identifier, streamEvent.GlobalOrdinal);
            if (OnEventReceived != null)
            {
                _ = OnEventReceived.Invoke(streamEvent);
            }
        });
    }

    public Task StartAsync()
    {
        _logger.LogInformation("Starting EventClient connection to {Url}", _url);
        return Connection.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await Connection.StopAsync();
        await Connection.DisposeAsync();
    }
}
