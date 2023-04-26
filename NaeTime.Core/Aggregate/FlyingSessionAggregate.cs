using EventSourcingCore.Aggregate.Abstractions;
using NaeTime.Abstractions.Event;
using NaeTime.Core.ValueTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Core.Aggregate
{
    public class FlyingSessionAggregate : AggregateRoot
    {
        private readonly List<Guid> _pilots = new List<Guid>();
        private Guid _hostId;
        private Guid _locationId;

        private readonly List<Flight> _flights = new List<Flight>();

        public FlyingSessionAggregate(Guid sessionId, Guid hostId, Guid locationId)
        {
            if (sessionId == Guid.Empty)
            {
                throw new DomainException("Session_Id_Cannot_Be_Empty");
            }
            if (hostId == Guid.Empty)
            {
                throw new DomainException("Host_Id_Cannot_Be_Empty");
            }
            if (locationId == Guid.Empty)
            {
                throw new DomainException("Location_Id_Cannot_Be_Empty");
            }

            Raise(new FlyingSessionStarted(sessionId, hostId, locationId));
        }

        public void When(FlyingSessionStarted e)
        {
            Id = e.FlyingSessionId;
            _hostId = e.HostId;
            _locationId = e.LocationId;
        }

        public void AddPilotToSession(Guid pilotId)
        {
            if (pilotId == Guid.Empty)
            {
                throw new DomainException("Pilot_Id_Cannot_Be_Empty");
            }

            Raise(new PilotJoinedFlyingSession(pilotId, Id));
        }

        public void When(PilotJoinedFlyingSession e)
        {
            _pilots.Add(e.PilotId);
        }

        public void StartFlight(Guid pilotId, int frequency, int frequencyBoundary)
        {
            var activeFlights = _flights.Where(x => x.Status != Flight.FlightStatus.Deactivated);

            if (activeFlights.Any(x => x.PilotId == pilotId))
            {
                throw new DomainException("Pilot_already_involved_in_flight");
            }

            if (activeFlights.Any(x => (x.Frequency - frequencyBoundary) <= frequency && frequency <= x.Frequency + frequencyBoundary))
            {
                throw new DomainException("Frequency_already_in_use");
            }

            int nextFlightId = 0;
            if (!_flights.Any())
            {
                nextFlightId = _flights.Select(x => x.FlightId).Max() + 1;
            }

            Raise(new FlightStarted(Id, nextFlightId, frequency));
        }

        public void ActivateFlight(int flightId)
        {
            var flight = _flights.FirstOrDefault(x => x.FlightId == flightId);
            if (flight != null)
            {
                if (flight.Status == Flight.FlightStatus.WaitingForSetup)
                {
                    Raise(new FlightActivated(Id, flightId));
                }
                else
                {
                    throw new DomainException("Flight_already_activated");
                }
            }
            else
            {
                throw new DomainException("FLight_not_found");
            }
        }

        public void When(FlightActivated e)
        {
            var flight = _flights.First(x => x.FlightId == e.FlightId);
            flight.Status = Flight.FlightStatus.Active;
        }

        public void CompleteFlight(int flightId)
        {
            var flight = _flights.FirstOrDefault(x => x.FlightId == flightId);
            if (flight != null)
            {
                if (flight.Status == Flight.FlightStatus.Active)
                {
                    Raise(new FlightCompleted(Id, flightId));
                }
                else
                {
                    throw new DomainException("Flight_already_activated");
                }
            }
            else
            {
                throw new DomainException("FLight_not_found");
            }
        }

        public void When(FlightCompleted e)
        {
            var flight = _flights.First(x => x.FlightId == e.FlightId);
            flight.Status = Flight.FlightStatus.Completed;
        }

        public void DeactivateFlight(int flightId)
        {
            var flight = _flights.FirstOrDefault(x => x.FlightId == flightId);
            if (flight != null)
            {
                if (flight.Status == Flight.FlightStatus.Active)
                {
                    Raise(new FlightDeactivated(Id, flightId));
                }
                else
                {
                    throw new DomainException("Flight_already_activated");
                }
            }
            else
            {
                throw new DomainException("FLight_not_found");
            }
        }

        public void When(FlightDeactivated e)
        {
            var flight = _flights.First(x => x.FlightId == e.FlightId);
            flight.Status = Flight.FlightStatus.Deactivated;
        }
    }
}
