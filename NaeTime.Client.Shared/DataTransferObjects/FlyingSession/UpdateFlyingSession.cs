﻿namespace NaeTime.Client.Shared.DataTransferObjects.FlyingSession;
public record UpdateFlyingSession(Guid Id, string Description, DateTime Start, DateTime ExpectedEnd, Guid TrackId);
