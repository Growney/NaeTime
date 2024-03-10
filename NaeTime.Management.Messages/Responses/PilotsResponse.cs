namespace NaeTime.Management.Messages.Responses;
public record PilotsResponse(IEnumerable<PilotsResponse.Pilot> Pilots)
{
    public record Pilot(Guid Id, string FirstName, string LastName, string CallSign);
}
