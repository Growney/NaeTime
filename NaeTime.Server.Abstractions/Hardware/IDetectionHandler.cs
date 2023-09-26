using NaeTime.Server.Abstractions.Models;

namespace NaeTime.Server.Abstractions.Hardware;
public interface IDetectionHandler
{
    public Task HandleDetectionAsync(Detection detection);
}
