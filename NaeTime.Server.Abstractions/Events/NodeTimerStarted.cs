namespace NaeTime.Server.Abstractions.Events;
public class NodeTimerStarted
{
    public Guid NodeId { get; set; }
    public Guid SessionId { get; set; }
}
