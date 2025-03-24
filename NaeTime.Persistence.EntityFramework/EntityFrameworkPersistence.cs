using NaeTime.Persistence.Abstractions;
using NaeTime.Persistence.Abstractions.Hardware;
using NaeTime.Persistence.Abstractions.Management;
using NaeTime.Persistence.Abstractions.OpenPractice;
using NaeTime.Persistence.Abstractions.Timing;

namespace NaeTime.Persistence.EntityFramework;

public class EntityFrameworkPersistence : INaeTimePersistence
{
    public IHardwareRepository Hardware { get; }

    public IOpenPracticeRepository OpenPractice { get; }

    public IManagementRepository Management { get; }

    public ITimingRepository Timing { get; }

    public EntityFrameworkPersistence(NaeTimeDbContext context)
    {
        Hardware = new EntityFrameworkHardwareRepository(context);
        OpenPractice = new EntityFrameworkOpenPracticeRepository(context);
        Management = new EntityFrameworkManagementRepository(context);
        Timing = new EntityFrameworkTimingRepository(context);
    }
}
