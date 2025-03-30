namespace NaeTime.Hardware.Node.Esp32.Abstractions;

public interface INodeManager
{
    public Task<bool> ConfigureLaneStatus(Guid timerId, byte laneId, bool isEnabled);
    public Task<bool> ConfigureLaneRadioFrequency(Guid timerId, byte laneId, byte? bandId, int frequencyInMhz);
    public Task<bool> ConfigureLaneEntryThreshold(Guid timerId, byte laneId, ushort threshold);
    public Task<bool> ConfigureLaneExitThreshold(Guid timerId, byte laneId, ushort threshold);
}
