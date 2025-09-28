using EventDbLite.Projections;
using NaeTime.Events;
using NaeTime.Query.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Query.Projections;
public class ActiveSession : Projection
{
    private Guid? _activeSessionId = null;
    private SessionType? _activeSessionType = null;

    private void When(OpenPracticeSessionActivated e)
    {
        _activeSessionId = e.SessionId;
        _activeSessionType = SessionType.OpenPractice;
    }
}
