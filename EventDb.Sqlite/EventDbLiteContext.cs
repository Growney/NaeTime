using Microsoft.EntityFrameworkCore;

namespace EventDb.Sqlite;

internal class EventDbLiteContext(DbContextOptions<EventDbLiteContext> options) : DbContext(options)
{
    public DbSet<DbModels.PersistedEvent> PersistedEvents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<DbModels.PersistedEvent>().HasKey(e => e.GlobalOrdinal);
        modelBuilder.Entity<DbModels.PersistedEvent>().Property(e => e.GlobalOrdinal).ValueGeneratedOnAdd();
        modelBuilder.Entity<DbModels.PersistedEvent>().HasIndex(e => e.StreamName).IsUnique(false);
        modelBuilder.Entity<DbModels.PersistedEvent>().Property(e => e.StreamName).IsRequired();
        modelBuilder.Entity<DbModels.PersistedEvent>().Property(e => e.StreamOrdinal).IsRequired();
        modelBuilder.Entity<DbModels.PersistedEvent>().HasIndex(e => new { e.StreamName, e.StreamOrdinal }).IsUnique();
        modelBuilder.Entity<DbModels.PersistedEvent>().Property(e => e.Metadata).IsRequired();
        modelBuilder.Entity<DbModels.PersistedEvent>().Property(e => e.Payload).IsRequired();
    }
}
