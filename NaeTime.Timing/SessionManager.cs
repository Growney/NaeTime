using NaeTime.Messages.Events.Timing.Practice;
using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
using NaeTime.PubSub;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Abstractions;
using NaeTime.Timing.Messages.Events;

namespace NaeTime.Timing;
public class SessionManager : ISubscriber
{
    private readonly IPublishSubscribe _publishSubscribe;
    private readonly ISessionFactory _sessionFactory;

    public SessionManager(IPublishSubscribe publishSubscribe, ISessionFactory sessionFactory)
    {
        _publishSubscribe = publishSubscribe ?? throw new ArgumentNullException(nameof(publishSubscribe));
        _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
    }

    public async Task When(LapStarted lap)
    {
        var currentSession = await _publishSubscribe.Request<ActiveSessionRequest, ActiveSessionResponse>(new ActiveSessionRequest());

        if (currentSession == null)
        {
            return;
        }

        var session = _sessionFactory.CreateSession(currentSession.SessionType, currentSession.SessionId);

        await session.HandleLapStarted(lap.TrackId, lap.Lane, lap.LapNumber, lap.SoftwareTime, lap.UtcTime, lap.HardwareTime);
    }
    public async Task When(LapCompleted lap)
    {
        var currentSession = await _publishSubscribe.Request<ActiveSessionRequest, ActiveSessionResponse>(new ActiveSessionRequest());

        if (currentSession == null)
        {
            return;
        }

        var session = _sessionFactory.CreateSession(currentSession.SessionType, currentSession.SessionId);

        await session.HandleLapCompleted(lap.TrackId, lap.Lane, lap.LapNumber, lap.SoftwareTime, lap.UtcTime, lap.HardwareTime, lap.TotalTime);
    }
    public async Task When(SplitStarted split)
    {
        var currentSession = await _publishSubscribe.Request<ActiveSessionRequest, ActiveSessionResponse>(new ActiveSessionRequest());

        if (currentSession == null)
        {
            return;
        }

        var session = _sessionFactory.CreateSession(currentSession.SessionType, currentSession.SessionId);

        await session.HandleSplitStarted(split.TrackId, split.Lane, split.LapNumber, split.Split, split.SoftwareTime, split.UtcTime);
    }
    public async Task When(SplitCompleted split)
    {
        var currentSession = await _publishSubscribe.Request<ActiveSessionRequest, ActiveSessionResponse>(new ActiveSessionRequest());

        if (currentSession == null)
        {
            return;
        }

        var session = _sessionFactory.CreateSession(currentSession.SessionType, currentSession.SessionId);

        await session.HandleSplitCompleted(split.TrackId, split.Lane, split.LapNumber, split.Split, split.SoftwareTime, split.UtcTime, split.TotalTime);
    }
    public async Task When(SplitSkipped split)
    {
        var currentSession = await _publishSubscribe.Request<ActiveSessionRequest, ActiveSessionResponse>(new ActiveSessionRequest());

        if (currentSession == null)
        {
            return;
        }

        var session = _sessionFactory.CreateSession(currentSession.SessionType, currentSession.SessionId);

        await session.HandleSplitSkipped(split.TrackId, split.Lane, split.LapNumber, split.Split);
    }
    public async Task When(PracticeSessionRequested session)
    {
        var currentSession = await _publishSubscribe.Request<ActiveSessionRequest, ActiveSessionResponse>(new ActiveSessionRequest());

        if (currentSession != null)
        {
            throw new NotSupportedException("Another session is already active");
        }

        await _publishSubscribe.Dispatch(new PracticeSessionActivated(session.SessionId));
    }
}
