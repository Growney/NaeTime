using NaeTime.Hardware.ImmersionRC.Models;

namespace NaeTime.Hardware.ImmersionRC.Abstractions;

public interface ILapRFManager
{
    public Task<LapRFLaneConfiguration?> GetTimerLaneConfiguration(Guid timerId, byte laneId);
    public Task<IEnumerable<LapRFLaneConfiguration>> GetTimeLaneConfigurations(Guid timerId);
    public Task<bool> IsTimerConnected(Guid timerId);
}
