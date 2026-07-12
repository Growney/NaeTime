namespace NaeTime.Client;
public static class OpenPracticeSessionProjector
{
    public static Models.OpenPracticeSession? Project(Query.Abstractions.Models.OpenPracticeSession? session)
    {
        if (session == null)
        {
            return null;
        }

        return new Models.OpenPracticeSession()
        {
            Id = session.Id,
            Name = session.Name,
            TrackId = session.TrackId,
            IsActive = session.IsActive,
            AttendingPilots = session.AttendingPilots,
            MinimumLapTime = session.MinimumLapTime,
            MaximumLapTime = session.MaximumLapTime,
            Lanes = session.Lanes.Select(l => new Models.OpenPracticeLane()
            {
                Lane = l.Lane,
                PilotId = l.PilotId,
                IsEnabled = l.IsEnabled,
                BandId = l.BandId,
                FrequencyInMHz = l.FrequencyInMHz
            }).ToList()
        };
    }
}
