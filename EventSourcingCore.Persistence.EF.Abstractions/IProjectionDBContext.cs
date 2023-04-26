using EventSourcingCore.Persistence.EF.Abstractions.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventSourcingCore.Persistence.EF.Abstractions
{
    public interface IProjectionDBContext
    {
        DbSet<StorePosition> StorePositions { get; set; }
        DbSet<StreamPosition> StreamPositions { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
