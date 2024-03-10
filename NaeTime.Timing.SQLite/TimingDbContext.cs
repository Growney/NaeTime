namespace NaeTime.Timing.SQLite;
internal class TimingDbContext : DbContext
{
    public DbSet<Lane> Lanes { get; set; }
    public DbSet<ActiveTimings> ActiveTimings { get; set; }
    public TimingDbContext(DbContextOptions<TimingDbContext> options) : base(options)
    {
    }
}
