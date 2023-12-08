namespace NaeTime.Client.Razor.Lib.Models;
public class TimerDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TimerType Type { get; set; }
}
