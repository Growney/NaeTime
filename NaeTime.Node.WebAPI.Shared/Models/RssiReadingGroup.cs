namespace NaeTime.Node.WebAPI.Shared.Models;

public class RssiReadingGroupDto
{
    public Guid NodeId { get; set; }
    public byte DeviceId { get; set; }
    public int Frequency { get; set; }
    public List<RssiReadingDto> Readings { get; set; } = new List<RssiReadingDto>();
}
