using NaeTime.Persistence.Client.Abstractions;
using NaeTime.Persistence.Client.Messages.Events;
using NaeTime.Persistence.Client.Messages.Requests;
using NaeTime.Persistence.Client.Messages.Responses;
using NaeTime.PubSub;

namespace NaeTime.Persistence.Client;
public class ClientModeService : ISubscriber
{
    private readonly ISimpleStorageProvider _storageProvider;
    public ClientModeService(ISimpleStorageProvider storageProvider)
    {
        _storageProvider = storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));
    }

    public async Task<ClientModeResponse> On(ClientModeRequest request)
    {
        var storedValue = await _storageProvider.GetAsync("Mode");

        if (!Enum.TryParse<ClientMode>(storedValue, out var storedMode))
        {
            storedMode = ClientMode.Local;
        }

        return new ClientModeResponse(storedMode);
    }

    public Task When(ClientModeConfigured clientModeConfigured)
    {
        return _storageProvider.SetAsync("Mode", clientModeConfigured.Mode.ToString());
    }
}
