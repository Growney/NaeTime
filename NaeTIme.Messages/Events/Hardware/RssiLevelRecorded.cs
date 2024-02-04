﻿namespace NaeTime.Messages.Events.Hardware;
public record RssiLevelRecorded(Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime, float Level);