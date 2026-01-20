using System;
using System.Collections.Generic;
using System.Text;

namespace NaeTime.Hardware.Abstractions;

public interface IRssiChannel
{
    public void AppendRssi(Guid timerId, byte lane, float rssi, long softwareTime, ulong? hardwareTime);
}
