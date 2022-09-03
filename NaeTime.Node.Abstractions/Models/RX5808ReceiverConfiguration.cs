using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Node.Abstractions.Models
{
    public class RX5808ReceiverConfiguration
    {
        public RX5808ReceiverConfiguration(Guid id, bool useAnalogToDigitalConverter, byte rSSIPin, byte dataPin, byte selectPin, byte clockPin)
        {
            Id = id;
            UseAnalogToDigitalConverter = useAnalogToDigitalConverter;
            RSSIPin = rSSIPin;
            DataPin = dataPin;
            SelectPin = selectPin;
            ClockPin = clockPin;
        }

        public Guid Id { get; }
        public bool UseAnalogToDigitalConverter { get; }
        public byte RSSIPin { get; }
        public byte DataPin { get; }
        public byte SelectPin { get; }
        public byte ClockPin { get; }
    }
}
