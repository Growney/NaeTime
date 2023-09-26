namespace NaeTime.Server.Abstractions.Events;
public class NodeTimerStopped
{
    public Guid NodeId { get; set; }
    public Guid SessionId { get; set; }
}
