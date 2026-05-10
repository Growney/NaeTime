using System;
using System.Collections.Generic;
using System.Text;

namespace NaeTime.Events;

public record ELRSBackpackInterfaceAdded(Guid Id, string Name, string ComPort);
public record ELRSBackpackInterfaceComPortReconfigured(Guid Id, string ComPort);
public record ELRSBackpackInterfaceRenamed(Guid Id, string Name);