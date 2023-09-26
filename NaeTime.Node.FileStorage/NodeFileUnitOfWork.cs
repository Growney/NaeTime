using NaeTime.Node.Abstractions.Repositories;

namespace NaeTime.Node.FileStorage;

public class NodeFileUnitOfWork : IUnitOfWork
{
    public IConfigurationRepository ConfigurationRepository => _configurationRepository;
    private readonly NodeFileConfigurationRepository _configurationRepository;

    public NodeFileUnitOfWork(NodeFileConfigurationRepository repository)
    {
        _configurationRepository = repository;
    }

    public Task CommitAsync() => _configurationRepository.CommitConfiguration();
}
