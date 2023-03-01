using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NaeTime.Abstractions.Models;
using NaeTime.Core.Models;

namespace NaeTime.Core
{
    public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
    {
        public DbSet<Node> Nodes => base.Set<Node>();
        public DbSet<Track> Tracks => base.Set<Track>();
        public DbSet<Flight> Flights => base.Set<Flight>();
        public DbSet<FlyingSession> FlyingSessions => base.Set<FlyingSession>();
        public DbSet<Pilot> Pilots => base.Set<Pilot>();
        public DbSet<RssiStreamReadingBatch> RssiReadingBatches => base.Set<RssiStreamReadingBatch>();

        public ApplicationDbContext(
            DbContextOptions options,
            IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ApplicationUser>().Property(x => x.PilotId).HasDefaultValueSql("NEWID()");

            base.OnModelCreating(builder);
        }
    }
}