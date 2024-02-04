﻿using NaeTime.Messages.Events.Timing;
using NaeTime.Persistence.Abstractions;
using NaeTime.PubSub;

namespace NaeTime.Persistence;
public class TimingService : ISubscriber
{
    private readonly IRepositoryFactory _repositoryFactory;

    public TimingService(IRepositoryFactory repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
    }

    public async Task When(LaneRadioFrequencyConfigured laneRadioFrequencyConfigured)
    {
        var timingRepository = await _repositoryFactory.CreateTimingRepository();

        await timingRepository.SetLaneRadioFrequency(laneRadioFrequencyConfigured.LaneNumber, laneRadioFrequencyConfigured.FrequencyInMhz);
    }
    public async Task When(LaneEnabled laneEnabled)
    {
        var timingRepository = await _repositoryFactory.CreateTimingRepository();
        await timingRepository.SetLaneStatus(laneEnabled.LaneNumber, true);
    }
    public async Task When(LaneDisabled laneDisabled)
    {
        var timingRepository = await _repositoryFactory.CreateTimingRepository();
        await timingRepository.SetLaneStatus(laneDisabled.LaneNumber, false);
    }
    public async Task When(LanePilotCleared lanePilotCleared)
    {
        var timingRepository = await _repositoryFactory.CreateTimingRepository();
        await timingRepository.SetLanePilot(lanePilotCleared.LaneNumber, null);
    }
    public async Task When(TimerDetectionOccured timerDetection)
    {
        var timingRepository = await _repositoryFactory.CreateTimingRepository();
        await timingRepository.AddTimerDetection(timerDetection.TimerId, timerDetection.Lane, timerDetection.HardwareTime, timerDetection.SoftwareTime, timerDetection.UtcTime);
    }
    public async Task When(TimerDetectionTriggered timerDetection)
    {
        var timingRepository = await _repositoryFactory.CreateTimingRepository();
        await timingRepository.AddManualDetection(timerDetection.TimerId, timerDetection.Lane, timerDetection.SoftwareTime, timerDetection.UtcTime);
    }
}
