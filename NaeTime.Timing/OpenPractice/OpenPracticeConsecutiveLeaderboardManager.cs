using NaeTime.Messages.Events.OpenPractice;
using NaeTime.Messages.Requests.OpenPractice;
using NaeTime.Messages.Responses;
using NaeTime.Messages.Responses.OpenPractice;
using NaeTime.PubSub;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Timing.OpenPractice;
public class OpenPracticeConsecutiveLeaderboardManager : ISubscriber
{
    private readonly IPublishSubscribe _publishSubscribe;

    public OpenPracticeConsecutiveLeaderboardManager(IPublishSubscribe publishSubscribe)
    {
        _publishSubscribe = publishSubscribe ?? throw new ArgumentNullException(nameof(publishSubscribe));
    }
    public async Task<OpenPracticeConsecutiveLapLeaderboardReponse> On(OpenPracticeConsecutiveLapLeaderboardRequest request)
    {
        var sessionRecords = await _publishSubscribe.Request<OpenPracticeSessionConsecutiveLapRecordsRequest, OpenPracticeSessionConsecutiveLapRecordsResponse>(new OpenPracticeSessionConsecutiveLapRecordsRequest(request.SessionId, request.LapCap));

        List<OpenPracticeConsecutiveLapLeaderboardReponse.LeadboardPosition> positions = new();
        if (sessionRecords != null)
        {
            var records = sessionRecords.Records.ToList();

            records.Sort(CompareRecords);

            for (int i = 0; i < records.Count; i++)
            {
                var record = records[i];
                positions.Add(new OpenPracticeConsecutiveLapLeaderboardReponse.LeadboardPosition(i, record.PilotId, record.TotalLaps, record.TotalMilliseconds, record.LastLapCompletionUtc, record.IncludedLaps));
            }
        }

        return new OpenPracticeConsecutiveLapLeaderboardReponse(positions);
    }

    private int CompareRecords(OpenPracticeSessionConsecutiveLapRecordsResponse.ConsecutiveLapRecord x, OpenPracticeSessionConsecutiveLapRecordsResponse.ConsecutiveLapRecord y)
    {
        if (x.TotalLaps == y.TotalLaps)
        {
            return x.TotalMilliseconds.CompareTo(y.TotalMilliseconds);
        }

        return y.TotalLaps.CompareTo(x.TotalLaps);
    }

    public Task When(OpenPracticeConsecutiveLapRecordImproved openPracticeConsecutiveLapRecordImproved)
    {

    }
}
