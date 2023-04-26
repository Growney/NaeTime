using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.Readiness.Abstractions
{
    public struct ReadinessResultBody
    {
        public ReadinessResultBody(bool success, string status)
        {
            Success = success;
            Status = status ?? throw new ArgumentNullException(nameof(status));
            Body = null;
        }
        public ReadinessResultBody(bool success, string status, IEnumerable<ReadinessResultBody> body)
        {
            Success = success;
            Status = status ?? throw new ArgumentNullException(nameof(status));
            Body = body;
        }
        public bool Success { get; }
        public string Status { get; }
        public IEnumerable<ReadinessResultBody> Body { get; }
    }
}
