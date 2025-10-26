namespace NaeTime.Client.Models;
public class OpenPracticeSession
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid TrackId { get; set; }
    public IEnumerable<Guid> TrackDetectorIds { get; set; } = Enumerable.Empty<Guid>();
    public IReadOnlyList<OpenPracticeLane> Lanes { get; set; } = new List<OpenPracticeLane>();
}
