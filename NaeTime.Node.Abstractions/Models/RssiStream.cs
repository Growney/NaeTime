using NaeTime.Node.Abstractions.Enumeration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Node.Abstractions.Models
{
    public class RssiStream
    {
        public RssiStream(Guid id, Guid receiverId, RssiReceiverType receiverType, long startTick, int tunedFrequency, int assignedFrequency)
        {
            Id = id;
            ReceiverId = receiverId;
            ReceiverType = receiverType;
            StartTick = startTick;
            TunedFrequency = tunedFrequency;
            AssignedFrequency = assignedFrequency;
        }

        public Guid Id { get; }
        public Guid ReceiverId { get; }
        public RssiReceiverType ReceiverType { get; }
        public long StartTick { get; }
        public int TunedFrequency { get; }
        public int AssignedFrequency { get; }
    }
}
