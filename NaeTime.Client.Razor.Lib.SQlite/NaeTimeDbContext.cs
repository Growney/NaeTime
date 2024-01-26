using Microsoft.EntityFrameworkCore;
using NaeTime.Client.Razor.Lib.SQlite.Models;

namespace NaeTime.Client.Razor.Lib.SQlite;
public class NaeTimeDbContext : DbContext
{
    public DbSet<EthernetLapRF8ChannelDetails> EthernetLapRF8Channels { get; set; }
    public DbSet<FlyingSessionDetails> FlyingSessions { get; set; }
    public DbSet<PilotDetails> Pilots { get; set; }
    public DbSet<TimedGateDetails> TimedGates { get; set; }
    public DbSet<TrackDetails> Tracks { get; set; }

    public NaeTimeDbContext(DbContextOptions<NaeTimeDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var trackEntity = modelBuilder.Entity<TrackDetails>();
        trackEntity.OwnsMany(x => x.TimedGates);
    }

}
