using NaeTime.Abstractions.Models;

namespace NaeTime.Abstractions.Handlers
{
    public interface IRssiStreamPassHander
    {
        void HandleStartedPass(RssiStreamPass pass);
        void HandleCompletedPass(RssiStreamPass pass);
    }
}
