using NaeTime.Messages.Events.Activation;
using NaeTime.Messages.Events.System;
using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
using NaeTime.PubSub;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Models;
using NaeTime.Timing.Practice;

namespace NaeTime.Timing;
public class SessionManager : ISubscriber
{
    private readonly IPublishSubscribe _publishSubscribe;

    public SessionManager(IPublishSubscribe publishSubscribe)
    {
        _publishSubscribe = publishSubscribe ?? throw new ArgumentNullException(nameof(publishSubscribe));
    }

    public async Task When(SystemStarted started)
    {
        var activeSession = await _publishSubscribe.Request<ActiveSessionRequest, ActiveSessionResponse>().ConfigureAwait(false);

        if (activeSession == null)
        {
            DisableAllSubscribers();
        }
        else
        {
            var sessionType = activeSession.Type switch
            {
                ActiveSessionResponse.SessionType.OpenPractice => SessionType.OpenPractice,
                _ => throw new NotImplementedException()
            };

            SetupForSessionType(sessionType);
        }
    }

    public Task When(SessionActivated activated)
    {
        var sessionType = activated.Type switch
        {
            SessionActivated.SessionType.OpenPractice => SessionType.OpenPractice,
            _ => throw new NotImplementedException()
        };

        SetupForSessionType(sessionType);

        return Task.CompletedTask;
    }
    public void SetupForSessionType(SessionType sessionType)
    {
        DisableAllSubscribers();
        ActivateSubsribersForSessionType(sessionType);
    }
    public void DisableAllSubscribers()
    {
        _publishSubscribe.DisableSubscriber<OpenPracticeSessionManager>();
    }
    public void ActivateSubsribersForSessionType(SessionType sessionType)
    {
        switch (sessionType)
        {
            case SessionType.OpenPractice:
                _publishSubscribe.EnableSubscriber<SessionDetectionManager>();
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
