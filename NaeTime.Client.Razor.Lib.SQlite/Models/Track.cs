namespace NaeTime.Client.Razor.Lib.SQlite.Models;
public class TrackDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IEnumerable<TimedGateDetails> TimedGates { get; set; } = Enumerable.Empty<TimedGateDetails>();
}
