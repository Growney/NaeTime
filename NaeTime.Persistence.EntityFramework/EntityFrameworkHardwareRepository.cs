using NaeTime.Persistence.Abstractions.Hardware;

namespace NaeTime.Persistence.EntityFramework;

public class EntityFrameworkHardwareRepository : IHardwareRepository
{
    private readonly NaeTimeDbContext _dbcontext;

    public EntityFrameworkHardwareRepository(NaeTimeDbContext dbcontext)
    {
        _dbcontext = dbcontext;
    }


}
