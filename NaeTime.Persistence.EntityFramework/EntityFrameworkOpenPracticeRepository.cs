using NaeTime.Persistence.Abstractions.OpenPractice;

namespace NaeTime.Persistence.EntityFramework;

public class EntityFrameworkOpenPracticeRepository : IOpenPracticeRepository
{
    private readonly NaeTimeDbContext _dbContext;

    public EntityFrameworkOpenPracticeRepository(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

}
