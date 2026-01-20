namespace NaeTime.Hardware.Node.Esp32.Abstractions;
internal interface INodeConnectionProvider
{
    public INodeConnection? GetNodeConnection(Guid timerId);
    public void SetNodeConnection(Guid timerId, INodeConnection connection);
}
