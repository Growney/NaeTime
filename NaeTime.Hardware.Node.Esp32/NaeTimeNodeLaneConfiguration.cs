using System;
using System.Collections.Generic;
using System.Text;

namespace NaeTime.Hardware.Node.Esp32;

public record NaeTimeNodeLaneConfiguration(byte Lane, byte? BandId, int FrequencyInMhz, bool IsEnabled, ushort EntryThreshold, ushort ExitThreshold);