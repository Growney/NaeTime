namespace NaeTime.Hardware.SQLite.Models;
public class TimerStatus
{
    public Guid Id { get; set; }
    public DateTime? ConnectionStatusChanged { get; set; }
    public bool WasConnected { get; set; }
}
