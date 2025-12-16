using System;
using System.Collections.Generic;
using System.Text;

namespace NaeTime.Events;

public record RssiRecorded(Guid TimerId, byte LaneId, float Rssi, long SoftwareTime, ulong? HardwareTime);
