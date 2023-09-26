using Mapping.Abstractions;
using Microsoft.Extensions.Logging;
using NaeTime.Node.Abstractions.Domain;
using NaeTime.Node.Abstractions.Models;

namespace NaeTime.Node.SignalR;
public class SignalRNodeClientFactory : INodeClientFactory
{

    private readonly IMapper _mapper;
    private readonly ILoggerFactory _loggerFactory;

    public SignalRNodeClientFactory(IMapper mapper, ILoggerFactory loggerFactory)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public async Task<INodeClient> CreateNodeClientAsync(NodeConfiguration configuration, CancellationToken token)
    {
        var logger = _loggerFactory.CreateLogger<SignalRNodeClient>();

        if (configuration.ServerAddress == null)
        {
            throw new InvalidOperationException("Service address not set");
        }

        var client = new SignalRNodeClient(configuration.ServerAddress, _mapper, logger);

        await client.InitializeAsync(token);

        return client;

    }
}
