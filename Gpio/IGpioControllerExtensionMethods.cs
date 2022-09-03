using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gpio
{
    public static class IGpioControllerExtensionMethods
    {

        public static void CheckAndClosePin(this IGpioController controller, int pin)
        {
            if (controller.IsPinOpen(pin))
            {
                controller.ClosePin(pin);
            }
        }
    }
}
