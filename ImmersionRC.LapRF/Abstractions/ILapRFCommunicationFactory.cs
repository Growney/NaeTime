using System.Net;

namespace ImmersionRC.LapRF.Abstractions;
public interface ILapRFCommunicationFactory
{
    ILapRFCommunication CreateEthernetCommunication(IPAddress address, int port);
}
