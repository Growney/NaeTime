using EventDbLite.Abstractions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EventDbLite.Reactions.SignalR.Server;
public class EventHubService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<Type> _requirements;
    private readonly ILiveProjectionRepository _repository;
    private readonly IHubContext<EventsHub> _hubContext;
    private readonly ILogger<EventHubService> _logger;

    public EventHubService(IServiceProvider serviceProvider, IEnumerable<Type> requirements, ILiveProjectionRepository repository, IHubContext<EventsHub> hubContext, ILogger<EventHubService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _requirements = requirements ?? throw new ArgumentNullException(nameof(requirements));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private async Task EnsureRequirementsAsync(long globalVersion, CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        List<Task> waitTasks = new();
        foreach (Type requirement in _requirements)
        {
            ILiveProjectionManager? projection = _repository.GetManager(requirement);
            if (projection is not null)
            {
                waitTasks.Add(projection.WaitForVersion(globalVersion, cancellationToken));
            }
        }
        await Task.WhenAll(waitTasks);
        stopwatch.Stop();
        _logger.LogInformation("Waited {ElapsedMilliseconds}ms for requirements at global version {GlobalVersion}", stopwatch.ElapsedMilliseconds, globalVersion);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();

        IEventStoreLite store = scope.ServiceProvider.GetRequiredService<IEventStoreLite>();

        IStreamSubscription subscription = store.SubscribeToAllStreams(StreamPosition.End);

        IEventSerializer serializer = scope.ServiceProvider.GetRequiredService<IEventSerializer>();

        await foreach (SubscriptionEvent streamEvent in subscription.StreamEvents(stoppingToken))
        {
            await EnsureRequirementsAsync(streamEvent.Event.GlobalOrdinal, stoppingToken);

            _logger.LogInformation("Broadcasting event {Identifier} with GlobalOrdinal {GlobalOrdinal}", streamEvent.Event.Data.Identifier, streamEvent.Event.GlobalOrdinal);
            await _hubContext.Clients.All.SendAsync("ReceiveEvent", streamEvent.Event);

        }
    }
}
