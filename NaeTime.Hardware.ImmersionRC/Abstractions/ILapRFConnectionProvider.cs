namespace NaeTime.Hardware.ImmersionRC.Abstractions;
public interface ILapRFConnectionProvider
{
    public ILapRFConnection? GetLapRFConnection(Guid timerId);
    public void SetLapRFConnection(Guid timerId, ILapRFConnection connection);
}
