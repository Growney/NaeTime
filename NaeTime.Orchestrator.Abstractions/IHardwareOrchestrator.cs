using System.Net;

namespace NaeTime.Orchestrator.Abstractions;

public interface IHardwareOrchestrator
{
    public Task DisconnectTimer(Guid timerId, DateTime disconnectionTime);
    public Task ConnectTimer(Guid timerId, DateTime connectionTime);
    public Task<Guid> CreateSerialEsp32Node(string name, string comPort);
    public Task<bool> ConfigureSerialEsp32Node(Guid id, string name, string comPort);
    public Task<Guid> CreateEthernetLapRF8(string name, IPAddress address, int port);
    public Task<bool> ConfigureEthernetLapRF8(Guid id, string name, IPAddress address, int port);

    public Task ConfigureLaneStatus(byte lane, bool isEnabled);
    public Task ConfigureTimerLaneStatus(Guid timerId, byte lane, bool isEnabled);
    public Task ConfigureLaneRadioFrequency(byte lane, byte? bandId, int frequencyInMhz);
    public Task ConfigureTimerLaneRadioFrequency(Guid timerId, byte lane, byte? bandId, int frequencyInMhz);

    public Task ConfigureLapRFLaneGain(Guid timerId, byte lane, ushort gain);
    public Task ConfigureLapRFLaneThreshold(Guid timerId, byte lane, float threshold);

    public Task ConfigureNodeEntryThreshold(Guid timerId, byte lane, ushort threshold);
    public Task ConfigureNodeExitThreshold(Guid timerId, byte lane, ushort threshold);
}
