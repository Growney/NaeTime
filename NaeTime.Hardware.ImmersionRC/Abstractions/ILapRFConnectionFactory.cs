using System.Net;

namespace NaeTime.Timing.ImmersionRC.Abstractions;
internal interface ILapRFConnectionFactory
{
    LapRFConnection CreateEthernetConnection(Guid timerId, IPAddress address, int port);
}
