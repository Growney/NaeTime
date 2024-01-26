namespace NaeTime.Client.Razor.Lib.SQlite.Models;
public class FlyingSessionDetails
{
    public Guid Id { get; set; }
    public string? Description { get; set; }
    public DateTime Start { get; set; }
    public DateTime ExpectedEnd { get; set; }
    public Guid? TrackId { get; set; }
}
