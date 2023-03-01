namespace NaeTime.Abstractions.Models
{
    public class RssiStreamReading
    {
        public Guid Id { get; set; }
        public Guid RssiStreamReadingBatchId { get; set; }
        public long Tick { get; set; }
        public int Value { get; set; }
    }
}
