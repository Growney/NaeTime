using System;
using System.Collections.Generic;
using System.Text;

namespace NaeTime.Hardware;

public struct RssiValue
{
    public Guid TimerId { get; }
    public byte Lane { get; }
    public float Rssi { get; }
    public long SoftwareTime { get; }
    public ulong? HardwareTime { get; }

    public RssiValue(Guid timerId, byte lane, float rssi, long softwareTime, ulong? hardwareTime)
    {
        TimerId = timerId;
        Lane = lane;
        Rssi = rssi;
        SoftwareTime = softwareTime;
        HardwareTime = hardwareTime;
    }
}
