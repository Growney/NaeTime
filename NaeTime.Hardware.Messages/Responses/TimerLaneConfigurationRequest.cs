﻿namespace NaeTime.Hardware.Messages.Responses;
public record TimerLaneConfigurationResponse(byte? bandId, int? FrequencyInMhz, bool IsEnabled);