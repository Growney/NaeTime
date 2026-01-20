using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Hardware.Node.Esp32.Abstractions;

public interface INodeConnection
{
    bool IsConnected { get; }

    ValueTask<bool> SetLaneEnabled(byte Lane, bool isEnabled);
    ValueTask<bool> SetLaneEntryThreshold(byte Lane, ushort threshold);
    ValueTask<bool> SetLaneExitThreshold(byte Lane, ushort threshold);
    ValueTask<bool> SetLaneRadioFrequency(byte Lane,byte? bandId, int frequencyInMhz);
    Task Start();
    Task Stop();

    Task<IEnumerable<NaeTimeNodeLaneConfiguration>> GetAllLaneConfigurations();
    Task<IEnumerable<NaeTimeNodeLaneConfiguration>> GetLaneConfigurations(IEnumerable<byte> lanes);
    Task<IEnumerable<NaeTimeNodeLaneConfiguration>> GetLaneConfigurations(params byte[] lanes);
}