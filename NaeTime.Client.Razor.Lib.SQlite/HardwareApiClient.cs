using Microsoft.EntityFrameworkCore;
using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Client.Razor.Lib.SQlite.Models;

namespace NaeTime.Client.Razor.Lib.SQlite;
internal class HardwareApiClient : IHardwareApiClient
{
    private readonly NaeTimeDbContext _context;

    public HardwareApiClient(NaeTimeDbContext context)
    {
        _context = context;
    }

    public async Task<EthernetLapRF8Channel?> CreateEthernetLapRF8ChannelAsync(string name, string ipAddress, int port)
    {
        var domain = new EthernetLapRF8ChannelDetails
        {
            Id = Guid.NewGuid(),
            Name = name,
            IpAddress = ipAddress,
            Port = port
        };

        _context.EthernetLapRF8Channels.Add(domain);

        await _context.SaveChangesAsync();

        return new EthernetLapRF8Channel(domain.Id, domain.Name, domain.IpAddress, domain.Port);

    }

    public async Task<IEnumerable<TimerDetails>> GetAllTimerDetailsAsync()
    {

        return await _context.EthernetLapRF8Channels.Select(x => new TimerDetails(x.Id, x.Name, TimerType.EthernetLapRF8Channel)).ToListAsync();
    }

    public async Task<EthernetLapRF8Channel?> GetEthernetLapRF8ChannelDetailsAsync(Guid id)
    {
        var stored = await _context.EthernetLapRF8Channels.FirstOrDefaultAsync(x => x.Id == id);

        if (stored == null)
        {
            return null;
        }

        return new EthernetLapRF8Channel(stored.Id, stored.Name, stored.IpAddress, stored.Port);

    }

    public async Task<EthernetLapRF8Channel?> UpdateEthernetLapRF8ChannelAsync(EthernetLapRF8Channel timer)
    {
        var stored = _context.EthernetLapRF8Channels.FirstOrDefault(x => x.Id == timer.Id);

        if (stored == null)
        {
            return null;
        }

        stored.Name = timer.Name;
        stored.IpAddress = timer.IpAddress;
        stored.Port = timer.Port;

        _context.EthernetLapRF8Channels.Update(stored);

        await _context.SaveChangesAsync();

        return timer;

    }
}
