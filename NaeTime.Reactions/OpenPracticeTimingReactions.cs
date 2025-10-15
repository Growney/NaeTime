using NaeTime.Command.Abstractions;

namespace NaeTime.Reactions;
internal class OpenPracticeTimingReactions(IOpenPracticeCommandHandler openPracticeCommandHandler, IOpenPracticePilotTimingCommandHandler openPracticePilotTimingCommandHandler)
{
    private readonly IOpenPracticeCommandHandler _openPracticeCommandHandler = openPracticeCommandHandler ?? throw new ArgumentNullException(nameof(openPracticeCommandHandler));
    private readonly IOpenPracticePilotTimingCommandHandler _openPracticePilotTimingCommandHandler = openPracticePilotTimingCommandHandler ?? throw new ArgumentNullException(nameof(openPracticePilotTimingCommandHandler));

    public async Task When(Events.HardwareDetectionAssignedToOpenPracticeSession assigned)
    {
        await _openPracticeCommandHandler.AddHardwareDetectionToSession(assigned.DetectionId, assigned.SessionId, assigned.TimerId, assigned.Lane, assigned.HardwareTime, assigned.SoftwareTime, assigned.UtcTime);
    }

    public async Task When(Events.OpenPracticeDetectionAssignedToPilot assigned)
    {
        await _openPracticePilotTimingCommandHandler.AddDetectionToPilot(assigned.PilotId, assigned.SessionId, assigned.DetectionId, assigned.OrdinalPosition, assigned.TrackDetectorCount, assigned.HardwareTime, assigned.SoftwareTime, assigned.UtcTime);
    }
}
