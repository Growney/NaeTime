namespace NaeTime.Server.Abstractions.Events;

public class RssiReadingReceived
{

    public Guid ReadingGroupId { get; set; }
    public long Tick { get; set; }
    public int Value { get; set; }
}
