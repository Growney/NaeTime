
using EventDbLite.Abstractions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace EventDbLite.Reactions.SignalR.Client;
public class EventClient : IEventClient
{
    private ConcurrentDictionary<int, Func<StreamEvent, Task>> _eventHandlers = new();

    public event Func<StreamEvent, Task>? OnEventReceived
    {
        add
        {
            if (value is null)
            {
                return;
            }
            int key = value.GetHashCode();
            _eventHandlers.TryAdd(key, value);
        }
        remove
        {
            if (value is null)
            {
                return;
            }
            int key = value.GetHashCode();
            _eventHandlers.TryRemove(key, out _);
        }
    }

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
            Console.WriteLine($"Event received: {streamEvent.Data.Identifier}");
            IEnumerable<Task> eventTasks = _eventHandlers.Values.Select(handler => handler(streamEvent));
            _ = Task.WhenAll(eventTasks);
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
