namespace ImmersionRC.LapRF;
public interface ILapRFProtocolFactory
{
    Task<ILapRFProtocol> Create(LapRFDeviceConfiguration configuration);
}
