using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.Store.Abstractions
{
    public struct StorePosition
    {
        public static readonly StorePosition Start = new StorePosition(ulong.MinValue, ulong.MinValue);
        public static readonly StorePosition End = new StorePosition(ulong.MaxValue, ulong.MaxValue);
        public StorePosition(ulong commitPosition, ulong preparePosition)
        {
            CommitPosition = commitPosition;
            PreparePosition = preparePosition;
        }

        public ulong CommitPosition { get; }
        public ulong PreparePosition { get; }

        public override bool Equals(object obj)
        {
            return obj is StorePosition position &&
                   CommitPosition == position.CommitPosition &&
                   PreparePosition == position.PreparePosition;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CommitPosition, PreparePosition);
        }

        public static bool operator ==(StorePosition left, StorePosition right)
        {
            return left.CommitPosition == right.CommitPosition && left.PreparePosition == right.PreparePosition;
        }
        public static bool operator !=(StorePosition left, StorePosition right)
        {
            return left.CommitPosition != right.CommitPosition || left.PreparePosition != right.PreparePosition;
        }
    }
}
