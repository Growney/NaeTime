namespace ImmersionRC.LapRF.Abstractions;

public interface ILapRFProtocolFactory
{
    ILapRFProtocol Create(ILapRFCommunication communication);
}
