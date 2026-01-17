using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Abstractions.Projections;
public interface INaeTimeNodeProjection
{
    NetworkNaeTimeNode? GetNetworkNaeTimeNode(Guid id);
    IEnumerable<NaeTimeNode> GetAllNaeTimeNodes();
    NaeTimeNodeLane? GetNaeTimeNodeLane(Guid timerId, byte laneId);
}
