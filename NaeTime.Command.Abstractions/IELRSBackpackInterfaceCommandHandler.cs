using System;
using System.Collections.Generic;
using System.Text;

namespace NaeTime.Command.Abstractions;

public interface IELRSBackpackInterfaceCommandHandler
{
    Task Add(Guid id, string name, string comPort);
    Task ReconfigureComPort(Guid id, string comPort);
    Task Rename(Guid id, string name);
}
