using EventDbLite.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Command.Aggregates;
public class PilotOpenPracticeTiming: AggregateRoot<string>
{
    public PilotOpenPracticeTiming(Guid pilotId, Guid sessionId)
    {
        Raise(new Events.PilotOpenPracticeSessionTimingStarted(pilotId, sessionId));
    }

    private void When(Events.PilotOpenPracticeSessionTimingStarted e)
    {
        Id = $"{e.PilotId}-{e.SessionId}";
    }
}
