using ImmersionRC.LapRF.Abstractions;
using System.Net;

namespace ImmersionRC.LapRF.Communication;
internal class LapRFCommunicationFactory : ILapRFCommunicationFactory
{
    public ILapRFCommunication CreateEthernetCommunication(IPAddress address, ushort port) => new LapRFEthernetCommunication(address, port);
}
