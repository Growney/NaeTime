using Gpio;
using Iot.Device.Adc;
using Microsoft.Extensions.Logging;
using NaeTime.Node.Abstractions.Domain;
using NaeTime.Node.Abstractions.Models;
using RX5808;
using System.Device.Gpio;
using System.Device.Spi;

namespace NaeTime.Node.Domain;

public class NodeDeviceFactory : INodeDeviceFactory, IDisposable
{
    private readonly GpioController _gpioController;
    private readonly ILogger<NodeDeviceFactory> _logger;
    public NodeDeviceFactory(ILogger<NodeDeviceFactory> logger)
    {
        _gpioController = new GpioController();
        _logger = logger;
    }

    public void Dispose()
    {
        _gpioController.Dispose();
    }

    public async Task<NodeDevices> GetNodeDevices(NodeConfiguration nodeConfiguration)
    {
        var disposables = new List<IDisposable>();

        var wrappedController = new WrappedGpioController(_gpioController);

        var analogToDigitalConverters = GetAnalogToDigitalConverters(nodeConfiguration, disposables);

        var multiplexerGroups = new Dictionary<byte, MultiplexedRx5808Group>();

        var rx5808Factory = new RX5808Factory(wrappedController);

        foreach (var multiplexerConfiguration in nodeConfiguration.MultiplexerConfigurations)
        {
            var multiplexerGroup = new MultiplexedRx5808Group(rx5808Factory, multiplexerConfiguration.Id,
                multiplexerConfiguration.AAddressPin,
                multiplexerConfiguration.BAddressPin,
                multiplexerConfiguration.CAddressPin,
                multiplexerConfiguration.DataPin,
                multiplexerConfiguration.SelectPin,
                multiplexerConfiguration.ClockPin);

            _logger.LogInformation($"Multiplexer found with pins A: {multiplexerConfiguration.AAddressPin} B: {multiplexerConfiguration.BAddressPin} C: {multiplexerConfiguration.CAddressPin} Data: {multiplexerConfiguration.DataPin} Select: {multiplexerConfiguration.SelectPin} Clock: {multiplexerConfiguration.ClockPin}");

            multiplexerGroups.Add(multiplexerConfiguration.Id, multiplexerGroup);
        }


        _logger.LogInformation("{deviceCount} multiplexers found", multiplexerGroups.Count);

        var tunedRssiDevices = new List<ITunedRssiDevice>();

        foreach (var multiplexedRx5808 in nodeConfiguration.MultiplexedRX5808Configurations)
        {
            if (!multiplexerGroups.TryGetValue(multiplexedRx5808.MultiplexerId, out var multiplexer))
            {
                throw new InvalidOperationException("Multiplexer configuration not found for device");
            }

            if (!analogToDigitalConverters.TryGetValue(multiplexedRx5808.ADCId, out var analogToDigitalConverter))
            {
                throw new InvalidOperationException("ADC configuration not found for device");
            }

            var multiplexedTuningComms = new MultiplexedRx5808TunningChannel(multiplexer, multiplexedRx5808.MultiplexerChannel);
            var rssiComms = new AnalogToDigitalConverterRssiCommunication(analogToDigitalConverter, multiplexedRx5808.ADCChannel);

            var tuningResult = await multiplexedTuningComms.TuneDeviceAsync(multiplexedRx5808.Frequency);

            if (tuningResult.Success)
            {
                _logger.LogInformation("Tuning device {deviceId} to frequency {frequency} was a success, actual tuned frequency was {actualFrequency}", multiplexedRx5808.Id, tuningResult.RequestedFrequency, tuningResult.TunedFrequency);
            }
            else
            {
                _logger.LogWarning("Tuning device {deviceId} to frequency {frequency} failed on multiplexer {multiplexerId}:{multiplexerChannel}", multiplexedRx5808.Id, multiplexedRx5808.Frequency, multiplexedRx5808.MultiplexerId, multiplexedRx5808.MultiplexerChannel);
            }

            tunedRssiDevices.Add(new Rx5808Device(multiplexedRx5808.Id, rssiComms, multiplexedTuningComms, multiplexedRx5808.IsEnabled));
        }
        _logger.LogInformation("Node configuration created {deviceCount} devices", tunedRssiDevices.Count);

        return new NodeDevices(tunedRssiDevices, disposables);
    }

    private Dictionary<byte, IAnalogToDigitalConverter> GetAnalogToDigitalConverters(NodeConfiguration nodeConfiguration, List<IDisposable> disposables)
    {
        Dictionary<byte, IAnalogToDigitalConverter> analogToDigitalConverters = new();
        foreach (var analogToDigitalConverterConfiguration in nodeConfiguration.AnalogToDigitalConverters)
        {
            var connectionSettings = new SpiConnectionSettings(analogToDigitalConverterConfiguration.BusId, analogToDigitalConverterConfiguration.ChipSelectLine)
            {
                ClockFrequency = analogToDigitalConverterConfiguration.ClockFrequency,
                Mode = GetSpiMode(analogToDigitalConverterConfiguration.SpiMode),
                DataBitLength = analogToDigitalConverterConfiguration.DataBitLength,
            };

            var spiDevice = SpiDevice.Create(connectionSettings);

            disposables.Add(spiDevice);

            var adc = new Mcp3008(spiDevice);

            var wrappedAdc = new WrappedMcp3008(adc);

            disposables.Add(wrappedAdc);

            analogToDigitalConverters.Add(analogToDigitalConverterConfiguration.Id, new WrappedMcp3008(adc));
        }

        _logger.LogInformation("{deviceCount} analog to digital converters found", analogToDigitalConverters.Count);

        return analogToDigitalConverters;
    }

    private static System.Device.Spi.SpiMode GetSpiMode(SpiMode mode)
        => mode switch
        {
            SpiMode.Mode1 => System.Device.Spi.SpiMode.Mode1,
            SpiMode.Mode2 => System.Device.Spi.SpiMode.Mode2,
            SpiMode.Mode3 => System.Device.Spi.SpiMode.Mode3,
            _ => System.Device.Spi.SpiMode.Mode0
        };
}
