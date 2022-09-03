namespace NaeTime.Abstractions.Models
{
    public class Node
    {

        public Guid Id { get; set; }
        public Guid? PilotId { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? Identifier { get; set; }
        public int RssiTransmissionDelay { get; set; }
        public int RssiRetryCount { get; set; }
        public List<RX5808> RX5808Receivers { get; set; } = new();
    }
}
