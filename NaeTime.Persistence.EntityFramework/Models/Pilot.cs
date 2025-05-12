namespace NaeTime.Persistence.EntityFramework.Models;
public class Pilot
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CallSign { get; set; }
    public string? BindingPhrase { get; set; }
}
