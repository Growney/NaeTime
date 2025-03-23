using NaeTime.Persistence.Abstractions.Hardware;

namespace NaeTime.Persistence.Abstractions;

public interface INaeTimePersistence
{
    public IHardwareRepository Hardware { get; }
}

