using ELRS.Backpack;
using Microsoft.Extensions.Hosting;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.PubSub.Abstractions;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace NaeTime.Hardware.ELRS;

public class BackpackManager : IHostedService
{
    private readonly IRemoteProcedureCallClient _rpcClient;
    private readonly IBackpackConnectionFactory _connectionFactory;
    private readonly IEventRegistrarScope _eventRegistrarScope;

    private readonly ConcurrentDictionary<Guid, BackpackConnector> _backpackConnectors = new();
    private readonly List<Guid> _backpackIds = new();
    private int _currentIndex = 0;

    public BackpackManager(IRemoteProcedureCallClient rpcClient, IBackpackConnectionFactory connectionFactory, IEventRegistrarScope eventRegistrarScope)
    {
        _rpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _eventRegistrarScope = eventRegistrarScope ?? throw new ArgumentNullException(nameof(eventRegistrarScope));

        _eventRegistrarScope.RegisterHub(this);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        IEnumerable<NaeTime.Hardware.Messages.Models.SerialELRSBackpack>? response = await _rpcClient.InvokeAsync<IEnumerable<NaeTime.Hardware.Messages.Models.SerialELRSBackpack>>("GetAllELRSBackpacks");

        if (response == null)
        {
            return;
        }

        foreach (NaeTime.Hardware.Messages.Models.SerialELRSBackpack backpack in response)
        {
            IBackpackConnection connection = _connectionFactory.Create(backpack.Port);
            BackpackConnector connector = new(backpack.Id, connection);
            _backpackConnectors.TryAdd(backpack.Id, connector);
            _backpackIds.Add(backpack.Id);
        }
    }

    private BackpackConnector? GetNextConnector()
    {
        if (_backpackConnectors.Count == 0)
        {
            return null;
        }

        BackpackConnector connector = _backpackConnectors[_backpackIds[_currentIndex]];
        _currentIndex = (_currentIndex + 1) % _backpackIds.Count;
        return connector;
    }

    public async Task When(OpenPracticeLapCompleted newLap)
    {
        NaeTime.Management.Messages.Models.Pilot? pilot = await _rpcClient.InvokeAsync<NaeTime.Management.Messages.Models.Pilot>("GetPilot", newLap.PilotId);

        if (pilot == null || string.IsNullOrWhiteSpace(pilot.BindingPhrase))
        {
            return;
        }

        BackpackConnector? backpackConnector = GetNextConnector();

        if (backpackConnector == null)
        {
            return;
        }

        byte[] Uid = HashPhrase(pilot.BindingPhrase);

        await backpackConnector.SendLap(Uid, TimeSpan.FromMilliseconds(newLap.TotalMilliseconds));

    }

    //to be moved to pilot creation
    public static byte[] HashPhrase(string bindPhrase)
    {
        // Create the input string
        string input = $"-DMY_BINDING_PHRASE=\"{bindPhrase}\"";

        // Compute the MD5 hash
        using MD5 md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

        // Take the first 6 bytes of the hash
        byte[] bindingPhraseHash = hash.Take(6).ToArray();

        // Adjust the first byte if it is odd
        if ((bindingPhraseHash[0] % 2) == 1)
        {
            bindingPhraseHash[0] -= 0x01;
        }

        return bindingPhraseHash;
    }
    public Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (KeyValuePair<Guid, BackpackConnector> backpack in _backpackConnectors)
        {
            backpack.Value.Stop();
        }

        return Task.CompletedTask;
    }
}
