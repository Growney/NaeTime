using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace EventDbLite.Reactions.SignalR.Server;

public class EventsHub : Hub
{
    private readonly ILogger<EventsHub> _logger;
    public EventsHub(ILogger<EventsHub> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public override Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }
}
