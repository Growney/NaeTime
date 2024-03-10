namespace NaeTime.Timing.SQLite;
internal class TimingDbContext : DbContext
{
    public DbSet<Lane> Lanes { get; set; }
    public DbSet<ActiveTimings> ActiveTimings { get; set; }
    public TimingDbContext(DbContextOptions<TimingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var timings = modelBuilder.Entity<ActiveTimings>();

        timings.OwnsOne(t => t.ActiveLap).WithOwner().HasForeignKey(x => x.ActiveTimingsId);
        timings.OwnsOne(t => t.ActiveSplit).WithOwner().HasForeignKey(x => x.ActiveTimingsId);

    }
}
