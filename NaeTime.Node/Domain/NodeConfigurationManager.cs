using NaeTime.Node.Abstractions.Domain;
using NaeTime.Node.Abstractions.Models;
using NaeTime.Node.Abstractions.Repositories;

namespace NaeTime.Node.Domain;

public class NodeConfigurationManager : INodeConfigurationManager
{
    public event Action<NodeConfiguration>? OnConfigurationChanged;

    private readonly IUnitOfWork _unitOfWork;

    public NodeConfigurationManager(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<NodeConfiguration?> GetCurrentConfigurationAsync()
        => _unitOfWork.ConfigurationRepository.GetNodeConfiguration();

}
