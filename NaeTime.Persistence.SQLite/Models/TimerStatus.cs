namespace NaeTime.Persistence.SQLite.Models;
public class TimerStatus
{
    public Guid TimerId { get; set; }
    public DateTime? ConnectionStatusChanged { get; set; }
    public bool WasConnected { get; set; }
}
