using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gpio;
using Iot.Device.Adc;

namespace RX5808
{
    public class RX5808ModuleWithAnalogToDigitalConverter : RX5808Module
    {
        private readonly IAnalogToDigitalConverter _adc;

        public RX5808ModuleWithAnalogToDigitalConverter(IAnalogToDigitalConverter adc, IGpioController controller, byte rssiPin, byte dataPin, byte selectPin, byte clockPin)
            : base(controller, rssiPin, dataPin, selectPin, clockPin)
        {
            _adc = adc ?? throw new ArgumentNullException(nameof(adc));

        }

        public override int ReadRssi()
        {
            return _adc.Read(_rssiPin);
        }
    }
}
