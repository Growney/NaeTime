namespace NaeTime.Persistence.SQLite.Models;
public class Pilot
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CallSign { get; set; }
}
