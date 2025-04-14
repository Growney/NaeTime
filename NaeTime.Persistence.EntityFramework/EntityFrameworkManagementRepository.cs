using NaeTime.Persistence.Abstractions.Management;

namespace NaeTime.Persistence.EntityFramework;

public class EntityFrameworkManagementRepository : IManagementRepository
{
    private readonly NaeTimeDbContext _dbContext;

    public EntityFrameworkManagementRepository(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

}
