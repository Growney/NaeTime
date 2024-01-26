namespace NaeTime.Client.Razor.Lib.Models;

public class Pilot
{
    public Pilot(Guid id, string? firstName, string? lastName, string? callSign)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        CallSign = callSign;
    }

    public Guid Id { get; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CallSign { get; set; }
}
