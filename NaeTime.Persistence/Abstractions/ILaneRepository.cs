using NaeTime.Persistence.Models;

namespace NaeTime.Persistence.Abstractions;
public interface ILaneRepository
{
    public Task<IEnumerable<Lane>> GetLanes();
    public Task SetLaneRadioFrequency(byte lane, byte? bandId, int frequencyInMhz);
    public Task SetLaneStatus(byte lane, bool isEnabled);
}
