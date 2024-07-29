namespace NaeTime.Client.Razor.Lib.Models;
public class LapRecord
{
    public uint LapCap { get; set; }
    public IEnumerable<Guid> IncludedLaps { get; set; } = Enumerable.Empty<Guid>();
}
