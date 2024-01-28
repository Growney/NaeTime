using Microsoft.EntityFrameworkCore;
using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.Razor.Lib.SQlite;
public class LocalSqliteApiClientProvider : ILocalApiClientProvider
{
    public string Description => "Local Db";

    public IHardwareApiClient HardwareApiClient { get; }

    public IPilotApiClient PilotApiClient { get; private set; }

    public IFlyingSessionApiClient FlyingSessionApiClient { get; private set; }

    public ITrackApiClient TrackApiClient { get; private set; }

    private readonly NaeTimeDbContext _context;

    public LocalSqliteApiClientProvider(IDispatcher dispatch)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NaeTimeDbContext>();
        optionsBuilder.UseSqlite($"Data Source=naetime.db");

        _context = new NaeTimeDbContext(optionsBuilder.Options);

        HardwareApiClient = new HardwareApiClient(_context);
        PilotApiClient = new PilotApiClient(_context);
        FlyingSessionApiClient = new FlyingSessionApiClient(_context);
        TrackApiClient = new TrackApiClient(_context);

        _ = Test(dispatch);

    }
    private async Task Test(IDispatcher dispatch)
    {
        while (true)
        {
            await dispatch.Dispatch(new Pilot(Guid.NewGuid(), "", "", ""));

            await Task.Delay(1000);
        }
    }

    public Task<bool> IsEnabledAsync(CancellationToken token) => Task.FromResult(true);

    public Task<bool> IsValidAsync(CancellationToken token) => Task.FromResult(true);

    public async Task<bool> TryConnectionAsync(CancellationToken token)
    {
        try
        {

            await _context.Database.EnsureCreatedAsync();

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}
