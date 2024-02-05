namespace NaeTime.Persistence.Models;
public class TimerDetails
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public TimerType Type { get; set; }
}
