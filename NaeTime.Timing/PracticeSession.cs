using NaeTime.Messages.Events.Timing.Practice;
using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Abstractions;

namespace NaeTime.Timing;
public class PracticeSession : ISession
{
    private readonly IPublishSubscribe _publishSubscribe;
    private readonly Guid _sessionId;

    public PracticeSession(IPublishSubscribe publishSubscribe, Guid sessionId)
    {
        _publishSubscribe = publishSubscribe ?? throw new ArgumentNullException(nameof(publishSubscribe));
        _sessionId = sessionId;
    }

    public async Task HandleLapCompleted(Guid trackId, byte lane, uint lapNumber, long softwareTime, DateTime utcTime, ulong? hardwareTime, long totalTime)
    {
        var laneConfigurations = await _publishSubscribe.Request<ActiveLaneConfigurationRequest, ActiveLaneConfigurationResponse>();

        var laneConfiguration = laneConfigurations?.Configurations.FirstOrDefault(lc => lc.Lane == lane);

        Guid? pilotId = laneConfiguration?.PilotId;

        var lapCompleted = new PracticeLapCompleted(_sessionId, lane, pilotId, lapNumber, softwareTime, utcTime, hardwareTime, totalTime);

        await _publishSubscribe.Dispatch(lapCompleted);
    }

    public async Task HandleLapStarted(Guid trackId, byte lane, uint lapNumber, long softwareTime, DateTime utcTime, ulong? hardwareTime)
    {
        var laneConfigurations = await _publishSubscribe.Request<ActiveLaneConfigurationRequest, ActiveLaneConfigurationResponse>();

        var laneConfiguration = laneConfigurations?.Configurations.FirstOrDefault(lc => lc.Lane == lane);

        Guid? pilotId = laneConfiguration?.PilotId;

        var lapCompleted = new PracticeLapStarted(_sessionId, lane, pilotId, lapNumber, softwareTime, utcTime, hardwareTime);

        await _publishSubscribe.Dispatch(lapCompleted);
    }

    public async Task HandleSplitCompleted(Guid trackId, byte lane, uint lapNumber, byte split, long softwareTime, DateTime utcTime, long totalTime)
    {
        var laneConfigurations = await _publishSubscribe.Request<ActiveLaneConfigurationRequest, ActiveLaneConfigurationResponse>();

        var laneConfiguration = laneConfigurations?.Configurations.FirstOrDefault(lc => lc.Lane == lane);

        Guid? pilotId = laneConfiguration?.PilotId;

        var lapCompleted = new PracticeSplitCompleted(_sessionId, lapNumber, split, lane, pilotId, softwareTime, utcTime, totalTime);

        await _publishSubscribe.Dispatch(lapCompleted);
    }

    public async Task HandleSplitSkipped(Guid trackId, byte lane, uint lapNumber, byte split)
    {
        var laneConfigurations = await _publishSubscribe.Request<ActiveLaneConfigurationRequest, ActiveLaneConfigurationResponse>();

        var laneConfiguration = laneConfigurations?.Configurations.FirstOrDefault(lc => lc.Lane == lane);

        Guid? pilotId = laneConfiguration?.PilotId;

        var lapCompleted = new PracticeSplitSkipped(_sessionId, lapNumber, split, lane, pilotId);

        await _publishSubscribe.Dispatch(lapCompleted);
    }

    public async Task HandleSplitStarted(Guid trackId, byte lane, uint lapNumber, byte split, long softwareTime, DateTime utcTime)
    {
        var laneConfigurations = await _publishSubscribe.Request<ActiveLaneConfigurationRequest, ActiveLaneConfigurationResponse>();

        var laneConfiguration = laneConfigurations?.Configurations.FirstOrDefault(lc => lc.Lane == lane);

        Guid? pilotId = laneConfiguration?.PilotId;

        var lapCompleted = new PracticeSplitStarted(_sessionId, lapNumber, split, lane, pilotId, softwareTime, utcTime);

        await _publishSubscribe.Dispatch(lapCompleted);
    }
}
