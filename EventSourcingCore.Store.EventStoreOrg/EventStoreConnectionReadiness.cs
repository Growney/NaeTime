using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventSourcingCore.Readiness.Abstractions;

namespace EventSourcingCore.Store.EventStoreOrg
{
    public class EventStoreConnectionReadiness : IReadinessCheck
    {
        private readonly IEventStoreConnectionWithStatus _connection;

        public EventStoreConnectionReadiness(IEventStoreConnectionWithStatus connection)
        {
            _connection = connection;
        }
        public async Task<ReadinessResult> IsReady()
        {
            await _connection.ConnectAsync();

            var body = new ReadinessResultBody(_connection.IsConnected, $"Connection Status: {_connection.ConnectionStatus}");

            return new ReadinessResult($"Event Store Org Connection", body);
        }


    }
}
