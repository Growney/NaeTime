using Microsoft.Extensions.DependencyInjection;
using NaeTime.Persistence.Abstractions;
using NaeTime.Persistence.Client.Messages.Requests;
using NaeTime.Persistence.Client.Messages.Responses;
using NaeTime.Persistence.SQLite;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Persistence.Client;
public class ClientRepositoryFactory : IRepositoryFactory
{
    private readonly IPublishSubscribe _pubSub;
    private readonly IServiceProvider _serviceProvider;

    public ClientRepositoryFactory(IPublishSubscribe pubSub, IServiceProvider serviceProvider)
    {
        _pubSub = pubSub ?? throw new ArgumentNullException(nameof(pubSub));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    private async Task<ClientMode> GetClientMode()
    {
        var currentModeResponse = await _pubSub.Request<ClientModeRequest, ClientModeResponse>();

        var currentMode = ClientMode.Local;
        if (currentModeResponse != null)
        {
            currentMode = currentModeResponse.Mode;
        }

        return currentMode;
    }

    public async Task<IHardwareRepository> CreateHardwareRepository()
    {
        var clientMode = await GetClientMode();

        return clientMode switch
        {
            ClientMode.Local => ActivatorUtilities.CreateInstance<SQLiteHardwareRepository>(_serviceProvider),
            _ => throw new InvalidOperationException($"Unknown client mode: {clientMode}")
        };
    }

    public async Task<IPilotRepository> CreatePilotRepository()
    {
        var clientMode = await GetClientMode();

        return clientMode switch
        {
            ClientMode.Local => ActivatorUtilities.CreateInstance<SQLitePilotRepository>(_serviceProvider),
            _ => throw new InvalidOperationException($"Unknown client mode: {clientMode}")
        };
    }

    public async Task<ITimingRepository> CreateTimingRepository()
    {
        var clientMode = await GetClientMode();

        return clientMode switch
        {
            ClientMode.Local => ActivatorUtilities.CreateInstance<SQLiteTimingRepository>(_serviceProvider),
            _ => throw new InvalidOperationException($"Unknown client mode: {clientMode}")
        };
    }

    public async Task<ITrackRepository> CreateTrackRepository()
    {
        var clientMode = await GetClientMode();

        return clientMode switch
        {
            ClientMode.Local => ActivatorUtilities.CreateInstance<SQLiteTrackRepository>(_serviceProvider),
            _ => throw new InvalidOperationException($"Unknown client mode: {clientMode}")
        };
    }

    public async Task<IActiveRepository> CreateActiveRepository()
    {
        var clientMode = await GetClientMode();

        return clientMode switch
        {
            ClientMode.Local => ActivatorUtilities.CreateInstance<SQLiteActiveRepository>(_serviceProvider),
            _ => throw new InvalidOperationException($"Unknown client mode: {clientMode}")
        };
    }
}
