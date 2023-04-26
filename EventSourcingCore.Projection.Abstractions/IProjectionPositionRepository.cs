using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventSourcingCore.Store.Abstractions;

namespace EventSourcingCore.Projection.Abstractions
{
    public interface IProjectionPositionRepository
    {
        Task SetStreamPosition(string key, string streamName, StreamPosition position);
        Task<StreamPosition?> GetStreamPosition(string key, string streamName);
        Task SetStorePosition(string key, StorePosition position);
        Task<StorePosition?> GetStorePosition(string key);
    }
}
