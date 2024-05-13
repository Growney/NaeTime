namespace NaeTime.Management.SQLite;
internal class ManagementDbContext : DbContext
{
    public DbSet<Pilot> Pilots { get; set; }
    public DbSet<Track> Tracks { get; set; }
    public DbSet<ActiveSession> ActiveSession { get; set; }

    public ManagementDbContext(DbContextOptions<ManagementDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Track> tracks = modelBuilder.Entity<Track>();
        tracks.OwnsMany(t => t.Timers).WithOwner().HasForeignKey(x => x.TrackId);

    }
}
