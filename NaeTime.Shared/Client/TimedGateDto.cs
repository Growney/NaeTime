namespace NaeTime.Shared.Client
{
    public class TimedGateDto
    {
        public Guid Id { get; set; }
        public Guid TrackId { get; set; }
        public int Position { get; set; }
        public Guid NodeId { get; set; }
        public RssiReceiverTypeDto? RssiReceiverType { get; set; }
    }
}
