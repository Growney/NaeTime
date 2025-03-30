using NaeTime.Hardware.ImmersionRC.Abstractions;
using NaeTime.Hardware.Node.Esp32.Abstractions;
using NaeTime.Orchestrator.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions.Events.Hardware;
using NaeTime.Orchestrator.Persistence.Abstractions;
using NaeTime.Persistence.Abstractions;
using NaeTime.Persistence.Abstractions.Hardware;
using System.Net;

namespace NaeTime.Orchestrator;

public class HardwareOrchestrator : IHardwareOrchestrator
{
    private readonly IHardwareOrchestratorPersistence _orchestratorPersistence;
    private readonly INaeTimeOrchestratorDistribution _distribution;
    private readonly INaeTimePersistence _persistence;

    private readonly ILapRFManager _lapRFManager;
    private readonly INodeManager _nodeManager;

    public HardwareOrchestrator(ILapRFManager lapRFManager,
        INodeManager nodeManager,
        INaeTimePersistence persistence,
        IHardwareOrchestratorPersistence orchestratorPersistence,
        INaeTimeOrchestratorDistribution distribution)
    {
        _orchestratorPersistence = orchestratorPersistence ?? throw new ArgumentNullException(nameof(orchestratorPersistence));
        _distribution = distribution ?? throw new ArgumentNullException(nameof(distribution));
        _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));

        _lapRFManager = lapRFManager ?? throw new ArgumentNullException(nameof(lapRFManager));
        _nodeManager = nodeManager ?? throw new ArgumentNullException(nameof(nodeManager));
    }

    public async Task ConnectTimer(Guid timerId, DateTime connectionTime)
    {
        await _orchestratorPersistence.ConnectTimer(timerId, connectionTime);

        await _distribution.Distribute(new TimerConnected(timerId));
    }

    public async Task<Guid> CreateEthernetLapRF8(string name, IPAddress address, int port)
    {
        Guid id = await _orchestratorPersistence.CreateEthernetLapRF8(name, address, port);

        await _distribution.Distribute(new EthernetLapRF8Created(id, name, address, port));

        return id;
    }

    public async Task<bool> ConfigureEthernetLapRF8(Guid id, string name, IPAddress address, int port)
    {
        bool result = await _orchestratorPersistence.ConfigureEthernetLapRF8(id, name, address, port);

        if (result)
        {
            await _distribution.Distribute(new EthernetLapRF8Configured(id, name, address, port));
        }

        return result;
    }
    public async Task<Guid> CreateSerialEsp32Node(string name, string comPort)
    {
        Guid id = await _orchestratorPersistence.CreateSerialEsp32Node(name, comPort);

        await _distribution.Distribute(new SerialEsp32NodeCreated(id, name, comPort));

        return id;
    }
    public async Task<bool> ConfigureSerialEsp32Node(Guid id, string name, string comPort)
    {
        bool result = await _orchestratorPersistence.ConfigureSerialEsp32Node(id, name, comPort);

        if (result)
        {
            await _distribution.Distribute(new SerialEsp32NodeConfigured(id, name, comPort));
        }

        return result;
    }
    public async Task DisconnectTimer(Guid timerId, DateTime disconnectionTime)
    {
        await _orchestratorPersistence.DisconnectTimer(timerId, disconnectionTime);

        await _distribution.Distribute(new TimerDisconnected(timerId));
    }
    private async Task SetTimersLaneStatus(byte lane, bool isEnabled)
    {
        IEnumerable<TimerDetails> timerDetails = await _persistence.Hardware.GetAllTimerDetails();

        foreach (TimerDetails timerDetail in timerDetails)
        {
            bool tuneResult = timerDetail.Type switch
            {
                TimerType.EthernetLapRF8Channel => await _lapRFManager.ConfigureLaneStatus(timerDetail.Id, lane, isEnabled),
                TimerType.SerialEsp32Node => await _nodeManager.ConfigureLaneStatus(timerDetail.Id, lane, isEnabled),
                TimerType.Unknown => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };

            await _orchestratorPersistence.StoreTimerActualLaneStatus(timerDetail.Id, lane, tuneResult ? isEnabled : !isEnabled);
        }
    }
    public async Task ConfigureLaneStatus(byte lane, bool isEnabled)
    {
        await _orchestratorPersistence.ConfigureSystemDesiredLaneStatus(lane, isEnabled);

        await SetTimersLaneStatus(lane, true);
    }
    public async Task ConfigureTimerLaneStatus(Guid timerId, byte lane, bool isEnabled)
    {
        await _orchestratorPersistence.ConfigureDesiredTimerLaneStatus(timerId, lane, isEnabled);
        TimerType timerType = await _persistence.Hardware.GetTimerType(timerId);
        bool tuneResult = timerType switch
        {
            TimerType.EthernetLapRF8Channel => await _lapRFManager.ConfigureLaneStatus(timerId, lane, isEnabled),
            TimerType.SerialEsp32Node => await _nodeManager.ConfigureLaneStatus(timerId, lane, isEnabled),
            TimerType.Unknown => throw new NotImplementedException(),
            _ => throw new NotImplementedException(),
        };
        if (tuneResult)
        {
            await _orchestratorPersistence.StoreTimerActualLaneStatus(timerId, lane, isEnabled);
        }
    }
    public async Task ConfigureLaneRadioFrequency(byte lane, byte? bandId, int frequencyInMhz)
    {
        await _orchestratorPersistence.ConfigureSystemDesiredLaneRadioFrequency(lane, frequencyInMhz);

        IEnumerable<TimerDetails> timerDetails = await _persistence.Hardware.GetAllTimerDetails();

        foreach (TimerDetails timerDetail in timerDetails)
        {
            bool tuneResult = timerDetail.Type switch
            {
                TimerType.EthernetLapRF8Channel => await _lapRFManager.ConfigureLaneRadioFrequency(timerDetail.Id, lane, bandId, frequencyInMhz),
                TimerType.SerialEsp32Node => await _nodeManager.ConfigureLaneRadioFrequency(timerDetail.Id, lane, bandId, frequencyInMhz),
                TimerType.Unknown => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };

            if (tuneResult)
            {
                await _orchestratorPersistence.StoreTimerActualLaneRadioFrequency(timerDetail.Id, lane, bandId, frequencyInMhz);
            }
        }
    }

    public async Task ConfigureTimerLaneRadioFrequency(Guid timerId, byte lane, byte? bandId, int frequencyInMhz)
    {
        await _orchestratorPersistence.ConfigureDesiredTimerLaneRadioFrequency(timerId, lane, bandId, frequencyInMhz);

        TimerType timerType = await _persistence.Hardware.GetTimerType(timerId);

        bool tuneResult = timerType switch
        {
            TimerType.EthernetLapRF8Channel => await _lapRFManager.ConfigureLaneRadioFrequency(timerId, lane, bandId, frequencyInMhz),
            TimerType.SerialEsp32Node => await _nodeManager.ConfigureLaneRadioFrequency(timerId, lane, bandId, frequencyInMhz),
            TimerType.Unknown => throw new NotImplementedException(),
            _ => throw new NotImplementedException(),
        };

        if (tuneResult)
        {
            await _orchestratorPersistence.StoreTimerActualLaneRadioFrequency(timerId, lane, bandId, frequencyInMhz);
        }
    }

    public async Task ConfigureLapRFLaneGain(Guid timerId, byte lane, ushort gain)
    {
        await _orchestratorPersistence.ConfigureDesiredLapRFLaneGain(timerId, lane, gain);

        bool result = await _lapRFManager.ConfigureLaneGain(timerId, lane, gain);

        if (result)
        {
            await _orchestratorPersistence.StoreActualLapRFLaneGain(timerId, lane, gain);
        }
    }
    public async Task ConfigureLapRFLaneThreshold(Guid timerId, byte lane, float threshold)
    {
        await _orchestratorPersistence.ConfigureDesiredLapRFLaneThreshold(timerId, lane, threshold);

        bool result = await _lapRFManager.ConfigureLaneThreshold(timerId, lane, threshold);

        if (result)
        {
            await _orchestratorPersistence.StoreActualLapRFLaneThreshold(timerId, lane, threshold);
        }
    }
    public async Task ConfigureNodeEntryThreshold(Guid timerId, byte lane, ushort threshold)
    {
        await _orchestratorPersistence.ConfigureDesiredNodeEntryThreshold(timerId, lane, threshold);

        bool result = await _nodeManager.ConfigureLaneEntryThreshold(timerId, lane, threshold);

        if (result)
        {
            await _orchestratorPersistence.StoreActualNodeEntryThreshold(timerId, lane, threshold);
        }
    }
    public async Task ConfigureNodeExitThreshold(Guid timerId, byte lane, ushort threshold)
    {
        await _orchestratorPersistence.ConfigureDesiredNodeExitThreshold(timerId, lane, threshold);

        bool result = await _nodeManager.ConfigureLaneExitThreshold(timerId, lane, threshold);

        if (result)
        {
            await _orchestratorPersistence.StoreActualNodeExitThreshold(timerId, lane, threshold);
        }
    }
}
