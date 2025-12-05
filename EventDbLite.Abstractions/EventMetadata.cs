namespace EventDbLite.Abstractions;

public class EventMetadata
{
    public Guid Id { get; set; }
    public DateTime InceptionUtc { get; set; }
    public string Identifier { get; set; } = string.Empty;
}
