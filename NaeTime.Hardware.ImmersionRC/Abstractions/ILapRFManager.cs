using NaeTime.Hardware.ImmersionRC.Models;

namespace NaeTime.Hardware.ImmersionRC.Abstractions;

public interface ILapRFManager
{
    public Task<LapRFLaneConfiguration?> GetTimerLaneConfiguration(Guid timerId, byte laneId);
    public Task<IEnumerable<LapRFLaneConfiguration>> GetTimeLaneConfigurations(Guid timerId);
    public Task<bool> IsTimerConnected(Guid timerId);
    public Task<bool> ConfigureLaneStatus(Guid timerId, byte laneId, bool isEnabled);
    public Task<bool> ConfigureLaneRadioFrequency(Guid timerId, byte laneId, byte? bandId, int frequencyInMhz);
    public Task<bool> ConfigureLaneGain(Guid timerId, byte laneId, ushort gain);
    public Task<bool> ConfigureLaneThreshold(Guid timerId, byte laneId, float threshold);

}
