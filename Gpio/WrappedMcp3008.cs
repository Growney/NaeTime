using Iot.Device.Adc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gpio
{
    public class WrappedMcp3008 : IAnalogToDigitalConverter
    {
        private readonly Mcp3008 _adc;

        public WrappedMcp3008(Mcp3008 adc)
        {
            _adc = adc;
        }

        public int Read(int channel)
        {
            return _adc.Read(channel);
        }
    }
}
