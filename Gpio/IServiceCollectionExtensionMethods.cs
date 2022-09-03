using Iot.Device.Adc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Spi;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gpio
{
    public static class IServiceCollectionExtensionMethods
    {
        public static IServiceCollection AddGpioController(this IServiceCollection services)
        {
            services.AddSingleton<GpioController>();
            services.AddSingleton<IGpioController>(provider => new WrappedGpioController(provider.GetRequiredService<GpioController>()));

            return services;
        }

        public static IServiceCollection AddStandardMcp3008(this IServiceCollection services)
        {
            services.AddSingleton<IAnalogToDigitalConverter>(provider =>
            {
                var connectionSettings = new SpiConnectionSettings(0, 0)
                {
                    ClockFrequency = 3_000_000,
                    Mode = SpiMode.Mode1,
                    DataBitLength = 8

                };

                var spiDevice = SpiDevice.Create(connectionSettings);
                var adc = new Mcp3008(spiDevice);
                return new WrappedMcp3008(adc);
            });
            return services;
        }
    }
}
