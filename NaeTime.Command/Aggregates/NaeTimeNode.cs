using EventDbLite.Aggregates;
using NaeTime.Events;

using System.Net;

namespace NaeTime.Command.Aggregates;
public class NaeTimeNode : AggregateRoot<Guid>
{
    private enum Type
    {
        Network,
        Serial,
    }

    private Type _type;
    private bool _isConnected;
    private byte _lanes;

    // lane related state removed - NaeTimeNode now tracks only timer-level configuration

    private string? _name;
    private string? _port;

    public NaeTimeNode() { }

    public NaeTimeNode(Guid id, string name, string port, byte lanes)
    {
        Raise(new NaeTimeNodeSerialEsp32NodeRegistered(id, name, port, lanes));
    }

    public NaeTimeNode(Guid id, string name, IPAddress address, ushort port, byte lanes)
    {
        Raise(new NaeTimeNodeNetworkDeviceRegistered(id, name, address.ToString(), port, lanes));
        Raise(new TimerRenamed(id, name));
    }

    // per-lane operations removed

    private void When(NaeTimeNodeSerialEsp32NodeRegistered e)
    {
        Id = e.TimerId;
        _name = e.Name;
        _port = e.Port;
        _type = Type.Serial;
        _lanes = e.Lanes;
    }

    private void When(NaeTimeNodeNetworkDeviceRegistered e)
    {
        Id = e.TimerId;
        _name = e.Name;
        _port = $"{e.IPAddress}:{e.Port}";
        _type = Type.Network;
        _lanes = e.Lanes;
    }

    public void MarkAsConnected()
    {
        Raise(new NaeTimeNodeTimerConnected(Id));
        Raise(new NaeTimeNodeConfigurationUnconfirmed(Id, _lanes));
        Raise(new TimerConnected(Id));
        
    }

    public void MarkAsDisconnected()
    {
        Raise(new NaeTimeNodeTimerDisconnected(Id));
        Raise(new NaeTimeNodeConfigurationUnconfirmed(Id, _lanes));
        Raise(new TimerDisconnected(Id));
        
    }

    private void When(NaeTimeNodeTimerConnected e)
    {
        _isConnected = true;
    }

    private void When(NaeTimeNodeTimerDisconnected e)
    {
        _isConnected = false;
    }

    private void When(NaeTimeNodeNetworkConfigurationChanged e)
    {
        _port = $"{e.IPAddress}:{e.Port}";
    }

    public void RenameDevice(string name)
    {
        if (_name == name) return;
        Raise(new NaeTimeNodeRenamed(Id, name));
    }

    public void ReconfigureSerialNode(string port)
    {
        if (_port == port) return;
        Raise(new NaeTimeNodeSerialEsp32ConfigurationChanged(Id, port));
    }
    public void ReconfigureNetworkNode(IPAddress address, ushort port)
    {
        string combined = $"{address}:{port}";
        if (_port == combined) return;
        Raise(new NaeTimeNodeNetworkConfigurationChanged(Id, address.ToString(), port));
    }
}
