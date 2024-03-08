using NaeTime.Persistence.Abstractions;

namespace NaeTime.Persistence;
internal class LeaderboardService
{
    private readonly IRepositoryFactory _repositoryFactory;
    public LeaderboardService(IRepositoryFactory repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
    }
}
