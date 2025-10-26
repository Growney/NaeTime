using NaeTime.Hardware.ImmersionRC.Models;

namespace NaeTime.Hardware.ImmersionRC.Abstractions;
public interface ILapRFConnection
{
    bool IsConnected { get; }

    Task<IEnumerable<LapRFLaneConfiguration>> GetAllLaneConfigurations();
    Task<IEnumerable<LapRFLaneConfiguration>> GetLaneConfigurations(IEnumerable<byte> lanes);
    Task<IEnumerable<LapRFLaneConfiguration>> GetLaneConfigurations(params byte[] lanes);
    Task SetLaneGain(byte lane, ushort gain);
    Task SetLaneRadioFrequency(byte Lane,byte? bandId, int frequencyInMhz);
    Task SetLaneStatus(byte Lane, bool isEnabled);
    Task SetLaneThreshold(byte lane, float threshold);
    Task SetupLane(LapRFLaneConfiguration configuration);
    Task Stop();
}