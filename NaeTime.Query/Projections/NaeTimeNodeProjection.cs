using NaeTime.Events.Domain;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Abstractions.Projections;
using System.Collections.Concurrent;
using System.Net;

namespace NaeTime.Query.Projections;
public class NaeTimeNodeProjection : INaeTimeNodeProjection
{
    private readonly ConcurrentDictionary<Guid, NaeTimeNode> _nodes = new();

    public IEnumerable<NaeTimeNode> GetAllNaeTimeNodes()
        => _nodes.Values;

    public NaeTimeNodeLane? GetNaeTimeNodeLane(Guid timerId, byte laneId)
    {
        if (!_nodes.TryGetValue(timerId, out var node)) return null;
        if (laneId >= node.Lanes.Length) return null;
        return node.Lanes[laneId];
    }

    public NetworkNaeTimeNode? GetNetworkNaeTimeNode(Guid id)
    {
        if (!_nodes.TryGetValue(id, out var node)) return null;
        return node as NetworkNaeTimeNode;
    }

    private void When(NaeTimeNodeSerialEsp32NodeRegistered e)
    {
        NaeTimeNodeLane[] lanes = new NaeTimeNodeLane[e.Lanes];
        for (byte i = 0; i < e.Lanes; i++)
        {
            lanes[i] = new NaeTimeNodeLane(i,
                new(null, null, false),
                new(null, null, false),
                new(null, null, false),
                new(null, null, false),
                new(null, null, false)
            );
        }

        var node = new NaeTimeNode(e.TimerId, e.Name, lanes);
        _nodes[e.TimerId] = node;
    }

    private void When(NaeTimeNodeNetworkDeviceRegistered e)
    {
        NaeTimeNodeLane[] lanes = new NaeTimeNodeLane[e.Lanes];
        for (byte i = 0; i < e.Lanes; i++)
        {
            lanes[i] = new NaeTimeNodeLane(i,
                new(null, null, false),
                new(null, null, false),
                new(null, null, false),
                new(null, null, false),
                new(null, null, false)
            );
        }

        var node = new NetworkNaeTimeNode(e.TimerId, e.Name, false, lanes, e.IPAddress, e.Port);
        _nodes[e.TimerId] = node;
    }

    private void When(NaeTimeNodeNetworkConfigurationChanged e)
    {
        if (_nodes.TryGetValue(e.TimerId, out var node) && node is NetworkNaeTimeNode net)
        {
            _nodes[e.TimerId] = net with { IPAddress = e.IPAddress, Port = e.Port };
        }
    }

    private void When(NaeTimeNodeRenamed e)
    {
        if (_nodes.TryGetValue(e.TimerId, out var node))
        {
            _nodes[e.TimerId] = node with { Name = e.Name };
        }
    }

    private void When(NaeTimeNodeLaneEnableRequested e)
    {
        var node = _nodes[e.TimerId];
        var lane = node.Lanes[e.Lane];
        node.Lanes[e.Lane] = lane with { IsEnabled = lane.IsEnabled with { Requested = true } };
    }

    private void When(NaeTimeNodeLaneDisableRequested e)
    {
        var node = _nodes[e.TimerId];
        var lane = node.Lanes[e.Lane];
        node.Lanes[e.Lane] = lane with { IsEnabled = lane.IsEnabled with { Requested = false } };
    }

    private void When(NaeTimeNodeLaneEnabled e)
    {
        var node = _nodes[e.TimerId];
        var lane = node.Lanes[e.Lane];
        node.Lanes[e.Lane] = lane with { IsEnabled = lane.IsEnabled with { Confirmed = true } };
    }

    private void When(NaeTimeNodeLaneDisabled e)
    {
        var node = _nodes[e.TimerId];
        var lane = node.Lanes[e.Lane];
        node.Lanes[e.Lane] = lane with { IsEnabled = lane.IsEnabled with { Confirmed = false } };
    }

    private void When(NaeTimeNodeLaneStatusMismatch e)
    {
        var node = _nodes[e.TimerId];
        var lane = node.Lanes[e.Lane];
        node.Lanes[e.Lane] = lane with { IsEnabled = lane.IsEnabled with { ConfirmedMismatch = true } };
    }

    private void When(NaeTimeNodeLaneFrequencyRequested e)
    {
        var node = _nodes[e.TimerId];
        var lane = node.Lanes[e.Lane];
        node.Lanes[e.Lane] = lane with
        {
            BandId = lane.BandId with { Requested = e.BandId },
            FrequencyInMHz = lane.FrequencyInMHz with { Requested = e.FrequencyInMHz }
        };
    }

    private void When(NaeTimeNodeLaneFrequencyTuned e)
    {
        var node = _nodes[e.TimerId];
        var lane = node.Lanes[e.Lane];
        node.Lanes[e.Lane] = lane with
        {
            BandId = lane.BandId with { Confirmed = e.BandId },
            FrequencyInMHz = lane.FrequencyInMHz with { Confirmed = e.FrequencyInMHz }
        };
    }

    private void When(NaeTimeNodeLaneFrequencyMismatch e)
    {
        var node = _nodes[e.TimerId];
        var lane = node.Lanes[e.Lane];
        node.Lanes[e.Lane] = lane with
        {
            BandId = lane.BandId with { ConfirmedMismatch = true },
            FrequencyInMHz = lane.FrequencyInMHz with { ConfirmedMismatch = true }
        };
    }

    private void When(NaeTimeNodeLaneEntryThresholdRequested e)
    {
        var node = _nodes[e.TimerId];
        var lane = node.Lanes[e.Lane];
        node.Lanes[e.Lane] = lane with { EntryThreshold = lane.EntryThreshold with { Requested = e.Threshold } };
    }

    private void When(NaeTimeNodeLaneEntryThresholdConfigured e)
    {
        var node = _nodes[e.TimerId];
        var lane = node.Lanes[e.Lane];
        node.Lanes[e.Lane] = lane with { EntryThreshold = lane.EntryThreshold with { Confirmed = e.Threshold } };
    }

    private void When(NaeTimeNodeLaneEntryThresholdMismatch e)
    {
        var node = _nodes[e.TimerId];
        var lane = node.Lanes[e.Lane];
        node.Lanes[e.Lane] = lane with { EntryThreshold = lane.EntryThreshold with { ConfirmedMismatch = true } };
    }

    private void When(NaeTimeNodeLaneExitThresholdRequested e)
    {
        var node = _nodes[e.TimerId];
        var lane = node.Lanes[e.Lane];
        node.Lanes[e.Lane] = lane with { ExitThreshold = lane.ExitThreshold with { Requested = e.Threshold } };
    }

    private void When(NaeTimeNodeLaneExitThresholdConfigured e)
    {
        var node = _nodes[e.TimerId];
        var lane = node.Lanes[e.Lane];
        node.Lanes[e.Lane] = lane with { ExitThreshold = lane.ExitThreshold with { Confirmed = e.Threshold } };
    }

    private void When(NaeTimeNodeLaneExitThresholdMismatch e)
    {
        var node = _nodes[e.TimerId];
        var lane = node.Lanes[e.Lane];
        node.Lanes[e.Lane] = lane with { ExitThreshold = lane.ExitThreshold with { ConfirmedMismatch = true } };
    }

    private void When(NaeTimeNodeTimerConnected e)
    {
        if (_nodes.TryGetValue(e.TimerId, out var node) && node is NetworkNaeTimeNode net)
        {
            _nodes[e.TimerId] = net with { IsConnected = true };
        }
    }

    private void When(NaeTimeNodeTimerDisconnected e)
    {
        if (_nodes.TryGetValue(e.TimerId, out var node) && node is NetworkNaeTimeNode net)
        {
            _nodes[e.TimerId] = net with { IsConnected = false };
        }
    }
}
