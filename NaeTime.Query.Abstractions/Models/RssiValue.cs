using System;
using System.Collections.Generic;
using System.Text;

namespace NaeTime.Query.Abstractions.Models;

public record RssiValue(Guid TimerId, byte Lane, float Rssi, long SoftwareTime, ulong? HardwareTime);