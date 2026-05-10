using EventDbLite.Aggregates;
using NaeTime.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace NaeTime.Command.Aggregates;

public class ELRSBackpackInterface : AggregateRoot<Guid>
{
    private string? _comPort;
    private string? _name;

    public ELRSBackpackInterface()
    {

    }
    public ELRSBackpackInterface(Guid id, string name, string comPort)
    {
        if (id == Guid.Empty)
            throw new InvalidOperationException("Invalid id");

        if (string.IsNullOrWhiteSpace(comPort))
            throw new InvalidOperationException("Invalid com port");

        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Invalid name");

        Raise(new ELRSBackpackInterfaceAdded(id, name, comPort));
    }

    private void When(ELRSBackpackInterfaceAdded added)
    {
        Id = added.Id;
        _comPort = added.ComPort;
        _name = added.Name;
    }
    public void ConfigureComPort(string comPort)
    {
        if (string.IsNullOrWhiteSpace(comPort))
            throw new InvalidOperationException("Invalid com port");

        if (comPort == _comPort)
            return;

        Raise(new ELRSBackpackInterfaceComPortReconfigured(Id, comPort));
    }
    private void When(ELRSBackpackInterfaceComPortReconfigured reconfigured)
    {
        _comPort = reconfigured.ComPort;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Invalid name");

        if (_name == name)
            return;

        Raise(new ELRSBackpackInterfaceRenamed(Id, name));
    }

    private void When(ELRSBackpackInterfaceRenamed rename)
    {
        _name = rename.Name;
    }
}
