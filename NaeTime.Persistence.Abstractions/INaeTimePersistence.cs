using NaeTime.Persistence.Abstractions.Hardware;
using NaeTime.Persistence.Abstractions.Management;
using NaeTime.Persistence.Abstractions.OpenPractice;
using NaeTime.Persistence.Abstractions.Timing;

namespace NaeTime.Persistence.Abstractions;

public interface INaeTimePersistence
{
    public IHardwareRepository Hardware { get; }
    public IOpenPracticeRepository OpenPractice { get; }
    public IManagementRepository Management { get; }
    public ITimingRepository Timing { get; }
}

