using NaeTime.Command.Abstractions;
using NaeTime.Events;

namespace NaeTime.Reactions;
internal class OpenPracticeTimingReactions
{
    private readonly IOpenPracticePilotTimingCommandHandler _openPracticePilotTimingCommandHandler;

    public OpenPracticeTimingReactions(IOpenPracticePilotTimingCommandHandler openPracticePilotTimingCommandHandler)
    {
        _openPracticePilotTimingCommandHandler = openPracticePilotTimingCommandHandler ?? throw new ArgumentNullException(nameof(openPracticePilotTimingCommandHandler));
    }

    public Task When(OpenPracticeSessionLanePilotSet lanePilot) =>
        _openPracticePilotTimingCommandHandler.StartPilotTimingSession(lanePilot.SessionId, lanePilot.PilotId);

    public Task When(OpenPracticeHardwareDetectionAssignedToPilot detection) =>
        _openPracticePilotTimingCommandHandler.AddDetectionOccurance(detection.DetectionId, detection.SessionId, detection.PilotId, detection.TimerId, detection.Lane, detection.HardwareTime, detection.SoftwareTime, detection.UtcTime);

}
