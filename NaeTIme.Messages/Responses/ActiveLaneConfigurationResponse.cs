using NaeTime.Messages.Models;

namespace NaeTime.Messages.Responses;
public record ActiveLaneConfigurationResponse(IEnumerable<LaneConfiguration> Configurations);