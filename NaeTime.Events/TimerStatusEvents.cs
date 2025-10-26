using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Events;
public record TimerRenamed(Guid TimerId, string NewName);
public record TimerConnected(Guid TimerId);
public record TimerDisconnected(Guid TimerId);
public record TimerLaneMismatch(Guid TimerId, byte LaneId);
public record TimerLanePending(Guid TimerId, byte LaneId);
public record TimerLaneConfirmed(Guid TimerId, byte LaneId);