using NaeTime.OpenPractice.Messages.Events;
using NaeTime.PubSub;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.OpenPractice;
public class OpenPracticeConsecutiveLeaderboardManager : ISubscriber
{
    private readonly IPublishSubscribe _publishSubscribe;

    public OpenPracticeConsecutiveLeaderboardManager(IPublishSubscribe publishSubscribe)
    {
        _publishSubscribe = publishSubscribe ?? throw new ArgumentNullException(nameof(publishSubscribe));
    }

    public Task When(ConsecutiveLapRecordImproved openPracticeConsecutiveLapRecordImproved)
    {
        throw new NotImplementedException();
    }
}
