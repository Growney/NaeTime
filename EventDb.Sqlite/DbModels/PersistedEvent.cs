namespace EventDb.Sqlite.DbModels;

internal class PersistedEvent
{
    public Guid Id { get; set; }
    public string StreamName { get; set; } = string.Empty;
    public long StreamOrdinal { get; set; }
    public long GlobalOrdinal { get; set; }
    public byte[] Metadata { get; set; } = Array.Empty<byte>();
    public byte[] Payload { get; set; } = Array.Empty<byte>();
    public string Identifier { get; set; } = string.Empty;
}
