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
    public class FlyingSessionRepository : IFlyingSessionRepository
    {
        private readonly ApplicationDbContext _context;

        public FlyingSessionRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public Task<List<FlyingSession>> GetAllowedAsync(Guid pilotId)
            => (from session in _context.FlyingSessions
                from allowedPilot in session.AllowedPilots
                where session.HostPilotId == pilotId || allowedPilot.PilotId == pilotId
                select session).ToListAsync();

        public Task<FlyingSession?> GetAsync(Guid id)
            => (from session in _context.FlyingSessions
                where session.Id == id
                select session).FirstOrDefaultAsync();

        public Task<List<FlyingSession>> GetAttendedAsync(Guid pilotId)
            => (from session in _context.FlyingSessions
                from flight in session.Flights
                where flight.PilotId == pilotId
                select session).ToListAsync();

        public Task<FlyingSession?> GetForFlightAsync(Guid flightId)
            => (from session in _context.FlyingSessions
                from flight in session.Flights
                where flight.Id == flightId
                select session).FirstOrDefaultAsync();

        public Task<List<FlyingSession>> GetForHostAsync(Guid hostPilotId)
            => (from session in _context.FlyingSessions
                where session.HostPilotId == hostPilotId
                select session).ToListAsync();

        public void Insert(FlyingSession session) => _context.FlyingSessions.Add(session);
        public void Update(FlyingSession session) => _context.FlyingSessions.Update(session);
    }
}
