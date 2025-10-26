using EventDbLite.Aggregates;
using NaeTime.Events;
using System.Net;

namespace NaeTime.Command.Aggregates;
public class ImmersionRCLapRF : AggregateRoot<Guid>
{
    private enum Type
    {
        Network,
        Serial,
    }


    private Type _type;

    public ImmersionRCLapRF()
    {

    }

    public ImmersionRCLapRF(Guid id, string name, IPAddress address, ushort port, byte lanes)
    {
        Raise(new ImmersionRCLapRFNetworkDeviceRegistered(id, name, address.ToString(), port, lanes));
        Raise(new TimerRenamed(id, name));
    }

    private void When(ImmersionRCLapRFNetworkDeviceRegistered changed)
    {
        Id = changed.TimerId;
        _type = Type.Network;
    }

    public void ConfigureNetwork(IPAddress address, ushort port)
    {
        if (_type != Type.Network)
        {
            throw new InvalidOperationException("Cannot configure network settings for a non-network device.");
        }

        Raise(new ImmersionRCLapRFNetworkConfigurationChanged(Id, address.ToString(), port));
    }

    public void Rename(string name)
    {
        Raise(new ImmersionRCLapRFRenamed(Id, name));
        Raise(new TimerRenamed(Id, name));
    }
    public void MarkAsConnected()
    {
        Raise(new ImmersionRCLapRFTimerConnected(Id));
        Raise(new ImmersionRCLapRFConfigurationUnconfirmed(Id));
        Raise(new TimerConnected(Id));
    }
    public void MarkAsDisconnected()
    {
        Raise(new ImmersionRCLapRFTimerDisconnected(Id));
        Raise(new ImmersionRCLapRFConfigurationUnconfirmed(Id));
        Raise(new TimerDisconnected(Id));
    }
}
