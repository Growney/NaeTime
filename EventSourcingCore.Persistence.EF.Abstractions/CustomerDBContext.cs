using EventSourcingCore.Persistence.EF.Abstractions.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Core.Security.Abstractions;

namespace EventSourcingCore.Persistence.EF.Abstractions
{
    public abstract class CustomerDBContext<TContext> : DbContext, IProjectionDBContext
        where TContext : CustomerDBContext<TContext>
    {
        private ICustomerContextAccessor _contextAccessor;

        public DbSet<StorePosition> StorePositions { get; set; }
        public DbSet<StreamPosition> StreamPositions { get; set; }

        public CustomerDBContext(DbContextOptions<TContext> options) : base(options)
        {
            _contextAccessor = this.GetService<ICustomerContextAccessor>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<StreamPosition>()
               .HasKey(o => new { o.Key, o.StreamName });

            var type = GetType();
            var method = type.GetMethod(nameof(ConfigureCustomerID));
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var nonCustomerAttribute = entityType.ClrType.GetCustomAttribute<NonCustomerEntityAttribute>();
                if (nonCustomerAttribute == null)
                {
                    var methodInfo = method.MakeGenericMethod(entityType.ClrType);
                    methodInfo.Invoke(this, new object[] { modelBuilder });
                }

            }
        }

        public void ConfigureCustomerID<T>(ModelBuilder builder)
        where T : class
        {
            builder.Entity<T>().Property<Guid>("CustomerID").HasValueGenerator<CustomerIDGenerator>();
            builder.Entity<T>().HasQueryFilter(e => _contextAccessor.Context.CustomerID != Guid.Empty && Microsoft.EntityFrameworkCore.EF.Property<Guid>(e, "CustomerID") == _contextAccessor.Context.CustomerID);
        }
    }
}
