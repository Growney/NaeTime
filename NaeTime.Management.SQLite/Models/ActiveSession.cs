namespace NaeTime.Management.SQLite.Models;
public class ActiveSession
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public SessionType SessionType { get; set; }

}
