using NaeTime.Announcer.Abstractions;
using NaeTime.Announcer.Models;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Announcer;
public class LeaderboardAnnouncerService
{
    private readonly IAnnouncmentQueue _queue;
    private readonly IRemoteProcedureCallClient _rpcClient;
    public LeaderboardAnnouncerService(IAnnouncmentQueue queue, IRemoteProcedureCallClient rpcClient)
    {
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _rpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
    }

    public Task When(ConsecutiveLapLeaderboardPositionImproved improved) => AnnounceNewConsecutiveLapRecord(improved.LapCap, improved.PilotId, improved.TotalLaps, improved.TotalMilliseconds, improved.NewPosition == 0, improved.WasTriggeredOnLapCompletion);
    public Task When(ConsecutiveLapLeaderboardRecordImproved improved) => AnnounceNewConsecutiveLapRecord(improved.LapCap, improved.PilotId, improved.TotalLaps, improved.TotalMilliseconds, improved.Position == 0, improved.WasTriggeredOnLapCompletion);
    public async Task AnnounceNewConsecutiveLapRecord(uint lapCap, Guid pilotId, uint totalLaps, long totalMilliseconds, bool isFastest, bool wasTriggeredByLapCompletion)
    {
        if (!wasTriggeredByLapCompletion)
        {
            return;
        }

        Management.Messages.Models.Pilot? pilot = await _rpcClient.InvokeAsync<Management.Messages.Models.Pilot>("GetPilot", pilotId);

        if (pilot == null)
        {
            return;
        }

        string identifier = pilot.CallSign ?? pilot.FirstName ?? pilot.LastName;
        TimeSpan totalTime = TimeSpan.FromMilliseconds(totalMilliseconds);

        string positionText = isFastest ? "fastest" : "personal best";

        string announcementText = $"{identifier} new {positionText} {lapCap} consecutive lap record with {totalLaps} laps in {totalTime.TotalSeconds}";

        _queue.Enqueue(new Announcement(announcementText));
    }

    public Task When(SingleLapLeaderboardPositionImproved improved) => AnnounceNewSingleLapRecord(improved.PilotId, improved.TotalMilliseconds, improved.NewPosition == 0, improved.WasTriggeredOnLapCompletion);

    public Task When(SingleLapLeaderboardRecordImproved improved) => AnnounceNewSingleLapRecord(improved.PilotId, improved.TotalMilliseconds, improved.Position == 0, improved.WasTriggeredOnLapCompletion);

    public async Task AnnounceNewSingleLapRecord(Guid pilotId, long totalMilliseconds, bool isFastest, bool wasTriggeredByLapCompletion)
    {
        if (!wasTriggeredByLapCompletion)
        {
            return;
        }

        Management.Messages.Models.Pilot? pilot = await _rpcClient.InvokeAsync<Management.Messages.Models.Pilot>("GetPilot", pilotId);

        if (pilot == null)
        {
            return;
        }

        string identifier = pilot.CallSign ?? pilot.FirstName ?? pilot.LastName;
        TimeSpan totalTime = TimeSpan.FromMilliseconds(totalMilliseconds);

        string positionText = isFastest ? "fastest" : "personal best";

        string announcementText = $"{identifier} new {positionText} single lap record in {totalTime.TotalSeconds}";

        _queue.Enqueue(new Announcement(announcementText));
    }
}
