namespace NaeTime.Client.Razor.Lib.Models;
public class FlyingSession
{
    public FlyingSession(Guid id, string? description, DateTime start, DateTime expectedEnd, Guid? trackId)
    {
        Id = id;
        Description = description;
        Start = start;
        ExpectedEnd = expectedEnd;
        TrackId = trackId;
    }

    public Guid Id { get; }
    public string? Description { get; set; }
    public DateTime Start { get; set; }
    public DateTime ExpectedEnd { get; set; }
    public Guid? TrackId { get; set; }
}
