using EventSourcingCore.Aggregate.Abstractions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using NaeTime.Abstractions.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Core.Aggregate
{
    public class RssiStreamAggregate : AggregateRoot
    {
        private HashSet<long> _usedTicks = new HashSet<long>();
        private bool _isFinished = false;

        public RssiStreamAggregate()
        {

        }

        public RssiStreamAggregate(Guid streamId)
        {
            if(streamId == Guid.Empty)
            {
                throw new DomainException("Stream_Id_Missing");
            }
            Raise(new RssiStreamStarted(streamId));
        }

        public void When(RssiStreamStarted e)
        {
            Id = e.StreamId;
        }

        public void LogRssiStreamValue(long tick, int value)
        {
            if (_usedTicks.Contains(tick))
            {
                throw new DomainException("Stream_Already_Contains_Tick");
            }

            if (_isFinished)
            {
                throw new DomainException("Stream_Finished");
            }

            Raise(new StreamRssiValueLogged(Id, tick, value));
        }

        public void When(StreamRssiValueLogged e)
        {
            _usedTicks.Add(e.Tick);
        }

        public void FinishStream()
        {
            if (_isFinished)
            {
                throw new DomainException("Stream_Alread_Finished");
            }

            Raise(new RssiStreamFinished(Id));
        }

        public void When(RssiStreamFinished e)
        {
            _isFinished = true;
        }
    }
}
