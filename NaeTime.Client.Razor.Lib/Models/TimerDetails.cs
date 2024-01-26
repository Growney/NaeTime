namespace NaeTime.Client.Razor.Lib.Models;
public class TimerDetails
{
    public TimerDetails(Guid id, string name, TimerType type)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Type = type;
    }

    public Guid Id { get; }
    public string Name { get; } = string.Empty;
    public TimerType Type { get; }
}
