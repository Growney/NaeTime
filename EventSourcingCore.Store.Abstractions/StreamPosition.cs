using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.Store.Abstractions
{
    public struct StreamPosition
    {
        public static readonly StreamPosition Start = new StreamPosition(ulong.MinValue);
        public static readonly StreamPosition End = new StreamPosition(ulong.MaxValue);
        public StreamPosition(ulong position)
        {
            Position = position;
        }

        public ulong Position { get; }

        public override bool Equals(object obj)
        {
            return obj is StreamPosition position &&
                   Position == position.Position;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Position);
        }

        public static bool operator ==(StreamPosition left, StreamPosition right)
        {
            return left.Position == right.Position;
        }
        public static bool operator !=(StreamPosition left, StreamPosition right)
        {
            return left.Position != right.Position;
        }

    }
}
