using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gpio
{
    public class WrappedGpioController : IGpioController
    {
        private readonly GpioController _controller;
        public WrappedGpioController(GpioController controller)
        {
            _controller = controller;
        }
        public void Dispose() => _controller.Dispose();

        public void OpenPin(int pinNumber, PinMode mode) => _controller.OpenPin(pinNumber, mode);
        public void OpenPin(int pinNumber, PinMode mode, PinValue initialValue) => _controller.OpenPin(pinNumber, mode, initialValue);
        public PinValue Read(int pinNumber) => _controller.Read(pinNumber);
        public void SetPinMode(int pinNumber, PinMode mode) => _controller.SetPinMode(pinNumber, mode);
        public void Write(int pinNumber, PinValue value) => _controller.Write(pinNumber, value);
        public void ClosePin(int pinNumber) => _controller.ClosePin(pinNumber);
        public bool IsPinOpen(int pinNumber) => _controller.IsPinOpen(pinNumber);
    }
}
