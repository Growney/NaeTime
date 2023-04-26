using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.Store.Abstractions
{
    public struct StreamState
    {
        public static readonly StreamState Any = new StreamState(ExpectedVersion.Any);
        public static readonly StreamState NoStream = new StreamState(ExpectedVersion.NoStream);
        public static readonly StreamState StreamExists = new StreamState(ExpectedVersion.StreamExists);

        private readonly int _state;

        private StreamState(int state)
        {
            _state = state;
        }

        public static bool operator ==(StreamState left, StreamState right)
        {
            return left._state == right._state;
        }
        public static bool operator !=(StreamState left, StreamState right)
        {
            return left._state != right._state;
        }
        public override bool Equals(object obj)
        {
            if (obj is StreamState state)
            {
                return _state == state._state;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _state;
        }
    }
}
