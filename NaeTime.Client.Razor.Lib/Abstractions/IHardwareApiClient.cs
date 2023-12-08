using NaeTime.Client.Razor.Lib.Models;

namespace NaeTime.Client.Razor.Lib.Abstractions;
public interface IHardwareApiClient
{
    Task<IEnumerable<TimerDetails>> GetAllTimerDetailsAsync();

    Task<IEnumerable<LapRF8Channel>> GetAllLapRF8ChannelAsync();
    Task<LapRF8Channel?> CreateLapRF8ChannelAsync(string name, int ipAddress, ushort port);
    Task<LapRF8Channel?> GetLapRF8ChannelDetailsAsync(Guid id);
}
