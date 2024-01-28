using ImmersionRC.LapRF.Abstractions;
using System.Net;

namespace ImmersionRC.LapRF.Communication;
internal class LapRFCommunicationFactory : ILapRFCommunicationFactory
{
    public ILapRFCommunication CreateEthernetCommunication(IPAddress address, int port) => new LapRFEthernetCommunication(address, port);
}
