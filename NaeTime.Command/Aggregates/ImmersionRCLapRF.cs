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
    private byte _lanes;

    public ImmersionRCLapRF()
    {

    }

    public ImmersionRCLapRF(Guid id, string name, IPAddress address, ushort port, byte lanes)
    {
        Raise(new ImmersionRCLapRFNetworkDeviceRegistered(id, name, address.ToString(), port, lanes));
    }

    private void When(ImmersionRCLapRFNetworkDeviceRegistered changed)
    {
        Id = changed.TimerId;
        _type = Type.Network;
        _lanes = changed.Lanes;
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
    }

    private void ThrowIfLaneNotExists(byte lane)
    {
        if (lane < 0 || lane >= _lanes)
        {
            throw new ArgumentException("Lane does not exist.", nameof(lane));
        }
    }

    public void MarkAsConnected()
    {
        Raise(new ImmersionRCLapRFTimerConnected(Id));
        Raise(new ImmersionRCLapRFConfigurationUnconfirmed(Id));
    }
    public void MarkAsDisconnected()
    {
        Raise(new ImmersionRCLapRFTimerDisconnected(Id));
        Raise(new ImmersionRCLapRFConfigurationUnconfirmed(Id));
    }
}
