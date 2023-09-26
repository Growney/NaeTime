namespace NaeTime.Node.Abstractions.Repositories;

public interface IUnitOfWork
{
    IConfigurationRepository ConfigurationRepository { get; }

    Task CommitAsync();
}
