using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.Projection.Core
{
    public class ProjectionManagerBuilderProvider : IProjectionManagerBuilderProvider
    {

        private readonly ConcurrentDictionary<string, IProjectionManagerBuilder> _repo = new ConcurrentDictionary<string, IProjectionManagerBuilder>();

        public bool AddBuilder(IProjectionManagerBuilder builder)
        {
            return _repo.TryAdd(builder.Key, builder);
        }

        public IEnumerable<IProjectionManagerBuilder> GetAll()
        {
            return _repo.Values;
        }

        public IProjectionManagerBuilder GetProjectionManager(string key)
        {
            _repo.TryGetValue(key, out var builder);

            return builder;
        }
    }
}
