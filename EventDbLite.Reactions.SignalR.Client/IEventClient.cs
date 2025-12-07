using EventDbLite.Abstractions;
using Microsoft.AspNetCore.SignalR.Client;

namespace EventDbLite.Reactions.SignalR.Client;
public interface IEventClient : IAsyncDisposable
{
    public event Func<StreamEvent, Task> OnEventReceived;

    public HubConnection Connection { get; }

    public Task StartAsync();
}
