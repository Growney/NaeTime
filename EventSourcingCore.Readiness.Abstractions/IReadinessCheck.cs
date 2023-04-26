using System;
using System.Threading.Tasks;

namespace EventSourcingCore.Readiness.Abstractions
{
    public interface IReadinessCheck
    {
        Task<ReadinessResult> IsReady();
    }
}
