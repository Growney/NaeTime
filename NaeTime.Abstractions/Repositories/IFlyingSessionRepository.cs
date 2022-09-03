using NaeTime.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Abstractions.Repositories
{
    public interface IFlyingSessionRepository
    {
        Task<FlyingSession?> GetAsync(Guid id);
        Task<FlyingSession?> GetForFlightAsync(Guid flightId);
        Task<List<FlyingSession>> GetForHostAsync(Guid hostPilotId);
        Task<List<FlyingSession>> GetAllowedAsync(Guid pilotId);
        Task<List<FlyingSession>> GetAttendedAsync(Guid pilotId);

        void Insert(FlyingSession session);
        void Update(FlyingSession session);
    }
}
