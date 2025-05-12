namespace NaeTime.Persistence.EntityFramework.Models
{
    public class SerialELRSBackpack
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Port { get; set; }
    }
}
