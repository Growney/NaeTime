using EventDbLite.Connections;
using EventDbLite.Streams;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventDbLite.Tests;

public class UnitTest1 : IDisposable
{
    private readonly EventDbLiteContext _context;
    private readonly IServiceProvider _serviceProvider;


    public UnitTest1()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddDbContext<EventDbLiteContext>(options =>
            options.UseSqlite($"Data Source=testeventdb.db"));
        services.AddScoped<EventStreamConnection>();
        _serviceProvider = services.BuildServiceProvider();

        _context = _serviceProvider.GetRequiredService<EventDbLiteContext>();
        _context.Database.Migrate();
    }

    public void Dispose() => _context.Database.EnsureDeleted();

    private long ReadCurrentVersion(EventStreamConnection connection, string streamName)
        => connection.ReadStreamEvents(streamName, StreamDirection.Forward, StreamPosition.Beginning).ToBlockingEnumerable().OrderByDescending(x => x.StreamOrdinal).Select(x => x.StreamOrdinal).FirstOrDefault();

    [Fact]
    public async Task Test1()
    {
        using var firstScope = _serviceProvider.CreateScope();
        using var secondScope = _serviceProvider.CreateScope();

        var firstConnection = firstScope.ServiceProvider.GetRequiredService<EventStreamConnection>();
        var secondConnection = secondScope.ServiceProvider.GetRequiredService<EventStreamConnection>();

        var eventData = new EventData([1, 2, 3, 4], [4, 5, 6, 7, 8]);

        long firstConnectionCurrentVersion = ReadCurrentVersion(firstConnection, "test-stream");
        long secondConnectionCurrentVersion = ReadCurrentVersion(secondConnection, "test-stream");
        var streamEvent = await firstConnection.AppendToStreamAsync("test-stream", eventData, StreamPosition.WithVersion(firstConnectionCurrentVersion + 1));

        await secondConnection.AppendToStreamAsync("test-stream", eventData, StreamPosition.Any);
    }
}
