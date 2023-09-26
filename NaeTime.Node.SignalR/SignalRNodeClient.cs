using Mapping.Abstractions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using NaeTime.Node.Abstractions.Domain;
using NaeTime.Node.Abstractions.Models;
using NaeTime.Node.SignalR.Shared;
using NaeTime.Node.WebAPI.Shared.Models;
using Tensor.Mapping.Abstractions;

namespace NaeTime.Node.SignalR;

public class SignalRNodeClient : INodeClient
{
    private const string c_hubPath = "NodeHub";
    private readonly HubConnection _connection;
    private readonly ILogger<SignalRNodeClient> _logger;
    private readonly IMapper _mapper;

    public SignalRNodeClient(string serverAddress, IMapper mapper, ILogger<SignalRNodeClient> logger)
    {
        _logger = logger;
        _mapper = mapper;

        _logger.LogInformation("Using configured server address", serverAddress);
        if (serverAddress.Last() != '/')
        {
            serverAddress += '/';
        }
        var nodeHubUri = serverAddress + c_hubPath;
        if (!Uri.TryCreate(nodeHubUri, UriKind.Absolute, out var validatedUri))
        {
            _logger.LogCritical("Unable to initialize Signal R client with server address {serverAddress}", validatedUri);
            throw new ArgumentException("Server Address Invalid");
        }
        _logger.LogInformation("Building Signal R client with server address {serverAddress}", validatedUri);
        _connection = new HubConnectionBuilder()
                  .WithUrl(validatedUri)
                  .Build();
    }

    public ValueTask DisposeAsync() => _connection.DisposeAsync();

    public async Task InitializeAsync(CancellationToken token)
    {
        //TODO hook up remote calls

        _logger.LogInformation("Starting Signal R Client");
        await _connection.StartAsync(token);
    }

    public Task SendInitializedAsync(NodeConfiguration configuration)
    {
        var configDto = _mapper.Map<NodeConfigurationDto>(configuration);
        return _connection.SendAsync(MethodNames.NodeConfigured, configDto);
    }

    public Task SendReadingsAsync(Guid nodeId, byte deviceId, int frequency, IEnumerable<RssiReading> readings)
    {
        var readingDtos = _mapper.Map<RssiReading, RssiReadingDto>(readings).ToList();

        var groupedDto = new RssiReadingGroupDto()
        {
            NodeId = nodeId,
            DeviceId = deviceId,
            Frequency = frequency,
            Readings = readingDtos
        };

        return _connection.SendAsync(MethodNames.RssiReadingGroup, groupedDto);
    }

    public Task SendTimerStartAsync(Guid nodeId, Guid sessionId) => _connection.SendAsync(MethodNames.TimerStarted, nodeId, sessionId);

    public Task SendTimerStoppedAsync(Guid nodeId, Guid sessionId) => _connection.SendAsync(MethodNames.TimerStopped, nodeId, sessionId);
}
