namespace NaeTime.Abstractions.Models
{
    public class RssiStreamReading
    {
        public Guid Id { get; set; }
        public Guid StreamId { get; set; }
        public long Tick { get; set; }
        public int Value { get; set; }
    }
}
