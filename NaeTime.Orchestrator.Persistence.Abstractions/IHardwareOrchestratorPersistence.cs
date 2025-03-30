using System.Net;

namespace NaeTime.Orchestrator.Persistence.Abstractions;

public interface IHardwareOrchestratorPersistence
{
    public Task DisconnectTimer(Guid timerId, DateTime disconnectionTime);
    public Task ConnectTimer(Guid timerId, DateTime connectionTime);
    public Task<Guid> CreateSerialEsp32Node(string name, string comPort);
    public Task<bool> ConfigureEthernetLapRF8(Guid id, string name, IPAddress address, int port);
    public Task<Guid> CreateEthernetLapRF8(string name, IPAddress address, int port);
    public Task<bool> ConfigureSerialEsp32Node(Guid id, string name, string comPort);

    public Task ConfigureSystemDesiredLaneStatus(byte lane, bool isEnabled);
    public Task ConfigureSystemDesiredLaneRadioFrequency(byte lane, int frequencyInMhz);

    public Task ConfigureDesiredTimerLaneStatus(Guid timerId, byte lane, bool isEnabled);
    public Task StoreTimerActualLaneStatus(Guid timerId, byte lane, bool isEnabled);
    public Task ConfigureDesiredTimerLaneRadioFrequency(Guid timerId, byte lane, byte? bandId, int frequencyInMhz);
    public Task StoreTimerActualLaneRadioFrequency(Guid timerId, byte lane, byte? bandId, int frequencyInMhz);

    public Task ConfigureDesiredLapRFLaneGain(Guid timerId, byte lane, ushort gain);
    public Task StoreActualLapRFLaneGain(Guid timerId, byte lane, ushort gain);
    public Task ConfigureDesiredLapRFLaneThreshold(Guid timerId, byte lane, float threshold);
    public Task StoreActualLapRFLaneThreshold(Guid timerId, byte lane, float threshold);

    public Task ConfigureDesiredNodeEntryThreshold(Guid timerId, byte lane, ushort threshold);
    public Task StoreActualNodeEntryThreshold(Guid timerId, byte lane, ushort threshold);
    public Task ConfigureDesiredNodeExitThreshold(Guid timerId, byte lane, ushort threshold);
    public Task StoreActualNodeExitThreshold(Guid timerId, byte lane, ushort threshold);
}
