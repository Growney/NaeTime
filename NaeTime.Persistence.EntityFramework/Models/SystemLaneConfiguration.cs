namespace NaeTime.Persistence.EntityFramework.Models;

public class SystemLaneConfiguration
{
    public Guid Id { get; set; }
    public byte LaneId { get; set; }
    public bool? IsEnabled { get; set; }
    public int? Frequency { get; set; }
    public byte? BandId { get; set; }
}
