using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Core.ValueTypes
{
    public class Flight
    {
        public Flight(int flightId,Guid pilotId, int frequency)
        {
            PilotId = pilotId;
            Frequency = frequency;
        }

        public enum FlightStatus
        {
            WaitingForSetup,
            Active,
            FailedToActive,
            Completed,
            Deactivated
        }
        public int FlightId { get; }
        public Guid PilotId { get; }
        public int Frequency { get; }
        public FlightStatus Status { get; set; } = FlightStatus.WaitingForSetup;

    }
}
