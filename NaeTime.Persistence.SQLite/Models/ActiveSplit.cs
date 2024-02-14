namespace NaeTime.Persistence.SQLite.Models;
public class ActiveSplit
{
    public Guid Id { get; set; }
    public Guid ActiveTimingsId { get; set; }
    public byte SplitNumber { get; set; }
    public long StartedSoftwareTime { get; set; }
    public DateTime StartedUtcTime { get; set; }
}
