using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Hardware;
public record RssiValue(Guid TimerId, byte LaneId, float Rssi, long SoftwareTime, ulong? HardwareTime);