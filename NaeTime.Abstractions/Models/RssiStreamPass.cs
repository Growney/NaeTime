namespace NaeTime.Abstractions.Models
{
    public class RssiStreamPass
    {
        public Guid Id { get; set; }
        public Guid RssiStreamId { get; set; }
        public long Start { get; set; }
        public long? End { get; set; }

        public long GetMidPoint()
        {
            if (End != null)
            {
                return Start + (End.Value - Start);
            }
            else
            {
                return Start;
            }
        }
    }
}
