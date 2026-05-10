namespace NaeTime.Hardware.ELRS.Abstractions;
public interface IBackpackConnectorProvider
{
    public IBackpackConnector? GetBackpackConnector(Guid id);
    public void SetBackpackConnector(Guid id, IBackpackConnector connector);
}
