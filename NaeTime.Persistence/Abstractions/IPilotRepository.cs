namespace NaeTime.Persistence.Abstractions;
public interface IPilotRepository
{
    public Task AddOrUpdatePilot(Guid id, string firstName, string lastName, string callSign);
}
