using Gpio;
using Iot.Device.Adc;
using Microsoft.Extensions.Logging;
using NaeTime.Node.Abstractions.Models;
using NaeTime.Node.Domain;
using RX5808;
using System.Device.Gpio;
using System.Device.Spi;

byte aAddressPin = 17;
byte bAddressPin = 27;
byte cAddressPin = 22;

byte dataPin = 26;
byte selectPin = 19;
byte clockPin = 13;

string? key;
do
{
    Console.WriteLine("Please input the test to run:");
    Console.WriteLine("1 = Frequency Tuning");
    Console.WriteLine("2 = Configuration");
    Console.WriteLine("3 = Multiplexed Register Communication");
    Console.WriteLine("q = Quit");

    key = Console.ReadLine();

    if (key == "1")
    {
        await FrequencyTuning(aAddressPin, bAddressPin, cAddressPin, dataPin, selectPin, clockPin);
    }
    else if (key == "2")
    {
        await TestConfiguration(aAddressPin, bAddressPin, cAddressPin, dataPin, selectPin, clockPin);
    }
    else if (key == "3")
    {

        await MultiplexedRegisterCommunication(aAddressPin, bAddressPin, cAddressPin, dataPin, selectPin, clockPin);
    }
}
while (key != "q");


static async Task MultiplexedRegisterCommunication(byte aAddress, byte bAddress, byte cAddress, byte dataPin, byte selectPin, byte clockPin)
{
    var gpioController = new GpioController();

    var wrappedController = new WrappedGpioController(gpioController);

    var rx5808Factory = new RX5808Factory(wrappedController);

    var multiplexedCommunication = rx5808Factory.GetMultiplexedModule(aAddress, bAddress, cAddress, dataPin, selectPin, clockPin);
    int maxFrequency = 5600;
    int minimumFrequency = 5600;
    int frequencyIncrement = 10;
    int currentFrequency = minimumFrequency;

    while (currentFrequency <= maxFrequency)
    {
        for (byte i = 0; i < 8; i++)
        {
            Console.WriteLine($"Setting Frequency Node: {i} {currentFrequency}");
            await multiplexedCommunication.PerformAction(i,
                x => x.SetFrequencyAsync(currentFrequency));
            int storedFrequency = await multiplexedCommunication.GetResult(i,
                x => x.GetActualStoredFrequencyAsync());
            Console.WriteLine($"Stored Frequency on Node: {i} {storedFrequency}");
        }
        currentFrequency += frequencyIncrement;
    }

}
static async Task TestConfiguration(byte aAddress, byte bAddress, byte cAddress, byte dataPin, byte selectPin, byte clockPin)
{
    var nodeConfiguration = new NodeConfiguration()
    {
        AnalogToDigitalConverters = new()
        {
            new()
            {
                Id=0,
                Mode = NaeTime.Node.Abstractions.Enumeration.AnalogToDigitalConverterMode.HardwareSPI,
                BusId = 0,
                ChipSelectLine = 0,
                ClockFrequency = 3_000_000,
                SpiMode = SpiMode.Mode1,
                DataBitLength = 8
            }
        },
        MultiplexerConfigurations = new()
        {
            new()
            {
                Id = 1,
                AAddressPin= aAddress,
                BAddressPin= bAddress,
                CAddressPin= cAddress,
                DataPin= dataPin,
                SelectPin= selectPin,
                ClockPin= clockPin
            }
        },
        MultiplexedRX5808Configurations = new()
        {
            new()
            {
                Id = 1,
                MultiplexerId = 1,
                MultiplexerChannel = 0,
                ADCId =0,
                ADCChannel = 0,
                Frequency = 5658,
                IsEnabled = true,
            },
            new()
            {
                Id = 2,
                MultiplexerId = 1,
                MultiplexerChannel = 1,
                ADCId =0,
                ADCChannel = 1,
                Frequency = 5695,
                IsEnabled = true,
            },
            new()
            {
                Id = 3,
                MultiplexerId = 1,
                MultiplexerChannel = 2,
                ADCId =0,
                ADCChannel = 2,
                Frequency = 5760,
                IsEnabled = true,
            },
            new()
            {
                Id = 4,
                MultiplexerId = 1,
                MultiplexerChannel = 3,
                ADCId =0,
                ADCChannel = 3,
                Frequency = 5800,
                IsEnabled = true,
            },
            new()
            {
                Id = 5,
                MultiplexerId = 1,
                MultiplexerChannel = 4,
                ADCId =0,
                ADCChannel = 4,
                Frequency = 5820,
                IsEnabled = true,
            },
            new()
            {
                Id = 6,
                MultiplexerId = 1,
                MultiplexerChannel = 5,
                ADCId =0,
                ADCChannel = 5,
                Frequency = 5780,
                IsEnabled = true,
            },
            new()
            {
                Id = 7,
                MultiplexerId = 1,
                MultiplexerChannel = 6,
                ADCId =0,
                ADCChannel = 6,
                Frequency = 5740,
                IsEnabled = true,
            },
            new()
            {
                Id = 8,
                MultiplexerId = 1,
                MultiplexerChannel = 7,
                ADCId =0,
                ADCChannel = 7,
                Frequency = 5700,
                IsEnabled = true,
            }
        }

    };

    var logFactory = LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
    });

    var logger = logFactory.CreateLogger<NodeDeviceFactory>();

    var nodeDeviceFactory = new NodeDeviceFactory(logger);

    var nodeDevices = await nodeDeviceFactory.GetNodeDevices(nodeConfiguration);


}
static async Task FrequencyTuning(byte aAddress, byte bAddress, byte cAddress, byte dataPin, byte selectPin, byte clockPin)
{
    var gpioController = new GpioController();

    var wrappedController = new WrappedGpioController(gpioController);

    var factory = new RX5808Factory(wrappedController);

    var multiplexed = factory.GetMultiplexedModule(aAddress, bAddress, cAddress, dataPin, selectPin, clockPin);

    var connectionSettings = new SpiConnectionSettings(0, 0)
    {
        ClockFrequency = 3_000_000,
        Mode = System.Device.Spi.SpiMode.Mode1,
        DataBitLength = 8

    };

    var spiDevice = SpiDevice.Create(connectionSettings);
    var adc = new Mcp3008(spiDevice);

    int maxFrequency = 5900;
    int minimumFrequency = 5600;
    int frequencyIncrement = 10;
    int currentFrequency = minimumFrequency;
    int delay = 10;

    while (true)
    {
        for (byte i = 0; i < 8; i++)
        {
            await Task.Delay(delay);
            Console.WriteLine($"Setting Frequency Node: {i} {currentFrequency}");
            await multiplexed.PerformAction(i,
                x => x.SetFrequencyAsync(currentFrequency));

            await Task.Delay(delay);
            await multiplexed.PerformAction(i,
                async x =>
                {
                    var frequency = await x.GetActualStoredFrequencyAsync();
                    Console.WriteLine($"Stored Frequency Node: {i} = {frequency}");
                });
        }

        for (byte i = 0; i < 8; i++)
        {
            await Task.Delay(delay);
            await multiplexed.PerformAction(i,
                async x =>
                {
                    var isConfirmed = await x.ConfirmFrequencyAsync(currentFrequency);
                    Console.WriteLine($"Confirmed Frequency Node: {i} = {isConfirmed}");
                });
            Console.WriteLine($"Rssi Node: {i} = {adc.Read(i)}");
        }

        currentFrequency += frequencyIncrement;
        if (currentFrequency > maxFrequency)
        {
            break;
        }
    }
}