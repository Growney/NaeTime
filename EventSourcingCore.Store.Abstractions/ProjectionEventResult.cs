using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Text;

namespace EventSourcingCore.Store.Abstractions
{
    public struct ProjectionEventResult
    {
        public static readonly ProjectionEventResult Success = new ProjectionEventResult(true);
        public static ProjectionEventResult FromMessage(string reason, NakAction action)
        {
            return new ProjectionEventResult(reason, action);
        }

        private ProjectionEventResult(bool acknowledge)
        {
            Acknowledge = acknowledge;
            Reason = null;
            Action = NakAction.Unknown;
        }
        private ProjectionEventResult(string reason, NakAction action)
        {
            Acknowledge = false;
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
            Action = action;
        }

        public bool Acknowledge { get; }
        public string Reason { get; }
        public NakAction Action { get; }
    }
}
