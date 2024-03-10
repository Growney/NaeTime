namespace NaeTime.OpenPractice.SQLite;

internal class OpenPracticeDbContext : DbContext
{
    public DbSet<OpenPracticeSession> OpenPracticeSessions { get; set; }
    public DbSet<ConsecutiveLapRecord> ConsecutiveLapRecords { get; set; }
    public DbSet<OpenPracticeLap> OpenPracticeLaps { get; set; }
    public OpenPracticeDbContext(DbContextOptions<OpenPracticeDbContext> options) : base(options)
    {
    }
}
