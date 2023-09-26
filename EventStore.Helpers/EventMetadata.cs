namespace EventStore.Helpers;

public class EventMetadata
{
    public DateTime Created { get; set; }
    public string TypeName { get; set; } = string.Empty;
}
