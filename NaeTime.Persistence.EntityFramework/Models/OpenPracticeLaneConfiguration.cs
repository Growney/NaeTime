namespace NaeTime.Persistence.EntityFramework.Models;
public class OpenPracticeLaneConfiguration
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public byte Lane { get; set; }
    public Guid? PilotId { get; set; }
    public bool IsEnabled { get; set; }
    public byte? BandId { get; set; }
    public int? FrequencyInMhz { get; set; }
}
