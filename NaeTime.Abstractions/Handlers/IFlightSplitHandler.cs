using NaeTime.Abstractions.Models;

namespace NaeTime.Abstractions.Handlers
{
    public interface IFlightSplitHandler
    {
        void HandleStartedSplit(Split split);
        void HandleCompletedSplit(Split split);

    }
}
