namespace NaeTime.Management.SQLite;
internal class ManagementDbContext : DbContext
{
    public DbSet<Pilot> Pilots { get; set; }
    public DbSet<Track> Tracks { get; set; }
    public DbSet<ActiveSession> ActiveSession { get; set; }

    public ManagementDbContext(DbContextOptions<ManagementDbContext> options) : base(options)
    {
    }
}
