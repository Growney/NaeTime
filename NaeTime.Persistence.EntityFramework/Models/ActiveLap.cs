namespace NaeTime.Persistence.EntityFramework.Models;
public class ActiveLap
{
    public Guid Id { get; set; }
    public Guid ActiveTimingsId { get; set; }
    public long StartedSoftwareTime { get; set; }
    public DateTime StartedUtcTime { get; set; }
    public ulong? StartedHardwareTime { get; set; }

}
