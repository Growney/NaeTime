namespace NaeTime.Shared.Client
{
    public class TrackDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public long MinimumLapTime { get; set; }

        public List<TimedGateDto> Gates { get; set; } = new List<TimedGateDto>();
    }
}
