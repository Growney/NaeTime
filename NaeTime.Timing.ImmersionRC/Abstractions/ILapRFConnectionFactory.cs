using System.Net;

namespace NaeTime.Timing.ImmersionRC.Abstractions;
public interface ILapRFConnectionFactory
{
    LapRFConnection CreateEthernetConnection(Guid timerId, IPAddress address, int port);
}
