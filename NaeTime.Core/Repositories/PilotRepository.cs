using Microsoft.EntityFrameworkCore;
using NaeTime.Abstractions.Models;
using NaeTime.Abstractions.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Core.Repositories
{
    public class PilotRepository : IPilotRepository
    {
        private readonly ApplicationDbContext _context;

        public PilotRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public Task<Pilot?> GetAsync(Guid id)
            => (from pilot in _context.Pilots
                where pilot.Id == id
                select pilot).FirstOrDefaultAsync();
    }
}
