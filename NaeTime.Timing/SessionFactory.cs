using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Abstractions;
using NaeTime.Timing.Models;

namespace NaeTime.Timing;
public class SessionFactory : ISessionFactory
{
    private readonly IPublishSubscribe _pubSub;
    public SessionFactory(IPublishSubscribe pubsub)
    {
        _pubSub = pubsub;
    }

    public ISession CreateSession(SessionType type, Guid id)
    {
        switch (type)
        {
            case SessionType.OpenPractice:
                {
                    var session = new PracticeSession(_pubSub, id);
                    return session;
                }
            default:
                throw new NotImplementedException();
        }
    }

}
