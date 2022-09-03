
using Gpio;
using Iot.Device.Adc;
using Iot.Device.Spi;
using RX5808;
using RX5808.Enumeration;
using System.Device.Gpio;
using System.Device.Spi;

//Yellow = Channel 1 = D11 = Data
//Green = Channel 2 = D10 = Select
//Blue = Channel 3 = D13 = Clock
//Orange = RSSI = A0 = Input

int minFrequency = 5650;
int maxFrequency = 5950;
int currentFrequency = minFrequency;
int frequencyStep = 20;
var gpioController = new GpioController();
var controller = new WrappedGpioController(gpioController);

var settings = new SpiConnectionSettings(0, 0)
{
    ClockFrequency = 3_000_000,
    Mode = SpiMode.Mode1,
    DataBitLength = 8
};

var spiDevice = SpiDevice.Create(settings);
var adc = new Mcp3008(spiDevice);
var wrappedAdc = new WrappedMcp3008(adc);

byte rssiPin = 4;
byte dataPin = 4;
byte selectPin = 3;
byte clockPin = 2;

using var module = new RX5808ModuleWithAnalogToDigitalConverter(wrappedAdc, controller, rssiPin, dataPin, selectPin, clockPin);
module.Initialise();


var enabledOptions = await module.GetEnabledOptions();
Console.WriteLine($"Currently Enabled Options: {enabledOptions}");



Console.WriteLine($"Writing Frequency {currentFrequency}Mhz");
await module.SetFrequencyAsync(currentFrequency);
await Task.Delay(1000);
bool confirmed = await module.ConfirmFrequencyAsync(currentFrequency);

if (confirmed)
{
    Console.WriteLine($"Frequency {currentFrequency}Mhz Confirmed");
}
else
{
    Console.WriteLine($"Frequency {currentFrequency}Mhz Failed");
}

await Task.Delay(1000);

while (true)
{
    Console.WriteLine(module.ReadRssi());
}
