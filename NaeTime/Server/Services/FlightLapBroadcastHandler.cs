using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using NaeTime.Abstractions;
using NaeTime.Abstractions.Handlers;
using NaeTime.Abstractions.Models;
using NaeTime.Server.Hubs;
using NaeTime.Shared.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Server.Services
{
    public class FlightLapBroadcastHandler : IFlightLapHandler
    {
        private readonly IHubContext<ClientHub> _hubContext;
        private readonly INaeTimeUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FlightLapBroadcastHandler(IHubContext<ClientHub> hubContext, INaeTimeUnitOfWork unitOfWork, IMapper mapper)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }


        public void HandleStartedLap(Lap lap)
        {
            _ = HandleLapAsync(lap, "OnStartedLap");
        }
        public void HandleCompletedLap(Lap lap)
        {
            _ = HandleLapAsync(lap, "OnCompletedLap");
        }

        private async Task HandleLapAsync(Lap lap, string message)
        {
            var pilotsToNotify = new List<Guid>();
            var lapDto = _mapper.Map<LapDto>(lap);
            
            var flight = await _unitOfWork.Flights.GetAsync(lap.FlightId);
            if (flight != null)
            {
                pilotsToNotify.Add(flight.PilotId);
                var pilot = await _unitOfWork.Pilots.GetAsync(flight.PilotId);
                
                if(pilot != null)
                {
                    var pilotDto = _mapper.Map<PilotDto>(pilot);

                    lapDto.Pilot = pilotDto;
                }

                var track = await _unitOfWork.Tracks.GetAsync(flight.TrackId);
                
                if(track != null)
                {
                    var trackDto = _mapper.Map<TrackDto>(track);

                    lapDto.Track = trackDto;
                }

                var session = await _unitOfWork.FlyingSessions.GetForFlightAsync(flight.Id);

                if(session != null)
                {
                    var sessionDto = _mapper.Map<FlyingSessionDto>(session);

                    lapDto.FlyingSession = sessionDto;

                    pilotsToNotify.Add(sessionDto.HostPilotId);
                    pilotsToNotify.AddRange(sessionDto.AllowedPilots.Select(x => x.Id));
                }
            }
            var groups = GetPilotGroups(pilotsToNotify);
            await _hubContext.Clients.Groups(groups).SendAsync(message, lapDto);
        }

        private IEnumerable<string> GetPilotGroups(List<Guid> pilotIds)
        {
            var uniqueIds = new HashSet<Guid>();

            foreach(var pilotId in pilotIds)
            {
                if (!uniqueIds.Contains(pilotId))
                {
                    uniqueIds.Add(pilotId); 
                }
            }
            return uniqueIds.Select(x=>$"Pilot-{x}");
        }
    }
}
