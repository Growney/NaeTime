using NaeTime.Command.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Reactions;
internal class OpenPracticeTimingReactions(IOpenPracticeCommandHandler openPracticeCommandHandler)
{
    private readonly IOpenPracticeCommandHandler _openPracticeCommandHandler = openPracticeCommandHandler ?? throw new ArgumentNullException(nameof(openPracticeCommandHandler));

    public async Task When(Events.DetectionAssignedToOpenPracticeSession assigned)
    {
        await _openPracticeCommandHandler.AddDetectionToSession(assigned.DetectionId, assigned.SessionId, assigned.Lane, assigned.HardwareTime, assigned.SoftwareTime, assigned.UtcTime);
    }

}
