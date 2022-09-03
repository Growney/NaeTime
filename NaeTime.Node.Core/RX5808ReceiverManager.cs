using Gpio;
using Iot.Device.Adc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NaeTime.Node.Abstractions;
using NaeTime.Node.Abstractions.Enumeration;
using NaeTime.Node.Abstractions.Models;
using RX5808;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Node.Core
{
    public class RX5808ReceiverManager : IRssiReceiverManager, IRX5808ReceiverManager
    {
        public event EventHandler? OnReceiversChanged;

        private readonly INodeTimeProvider _timeProvider;
        private readonly IGpioController _gpioController;
        private readonly IAnalogToDigitalConverter _adc;
        private readonly ILogger<RX5808ReceiverManager> _logger;

        private List<RX5808Receiver> _receivers = new List<RX5808Receiver>();
        public RssiReceiverType ReceiverType => RssiReceiverType.RX5808;

        public RX5808ReceiverManager(INodeTimeProvider timeProvider, IGpioController gpioController, IAnalogToDigitalConverter adc, ILogger<RX5808ReceiverManager> logger)
        {
            _timeProvider = timeProvider;
            _adc = adc;
            _gpioController = gpioController;
            _logger = logger;
        }

        public async Task SetupReceivers(IEnumerable<RX5808ReceiverConfiguration> receivers)
        {

            foreach (var receiver in _receivers)
            {
                receiver.Dispose();
            }
            _receivers.Clear();
            _logger.LogInformation($"Adding {receivers.Count()} RX5808 receivers");
            foreach (var configuration in receivers)
            {
                var newReceiver = CreateReceiver(configuration.Id, configuration.UseAnalogToDigitalConverter, configuration.RSSIPin, configuration.DataPin, configuration.SelectPin, configuration.ClockPin);
                _receivers.Add(newReceiver);
            }


            foreach (var receiver in _receivers)
            {
                await receiver.InitialiseAsync();
            }

            OnReceiversChanged?.Invoke(this, new EventArgs());
        }

        private RX5808Receiver CreateReceiver(Guid id, bool useADC, byte rssiPin, byte dataPin, byte selectPin, byte clockPin)
        {
            var module = useADC ?
                new RX5808ModuleWithAnalogToDigitalConverter(_adc, _gpioController, rssiPin, dataPin, selectPin, clockPin)
                :
                new RX5808Module(_gpioController, rssiPin, dataPin, selectPin, clockPin);

            var receiver = new RX5808Receiver(id, module);

            return receiver;
        }
        public IEnumerable<IRssiReceiver> GetEnabledReceivers() => _receivers.Where(x => x.CurrentStream != null);
        public IEnumerable<IRssiReceiver> GetReceivers() => _receivers;
        public bool IsStreamManaged(Guid streamId) => _receivers.Any(x => x.CurrentStream?.Id == streamId);
        public bool CanHandleNewStream(int frequency)
        {
            return CanHandleFrequency(frequency)
                && HasFreeReceivers();
        }

        private bool CanHandleFrequency(int frequency)
        {
            //TODO check for valid frequency
            return true;
        }
        private bool HasFreeReceivers() => _receivers.Any(x => x.CurrentStream == null);
        public RssiStream? GetStream(Guid streamId)
        {
            var streamReceiver = _receivers.FirstOrDefault(x => x.CurrentStream?.Id == streamId);
            return streamReceiver?.CurrentStream;
        }
        public async Task<RssiStream?> EnableStreamAsync(Guid streamId, int frequency)
        {
            var receivers = _receivers.Where(x => x.CurrentStream == null);
            _logger.LogInformation($"{receivers.Count()} free receivers found for new stream {streamId} on {frequency}mhz");
            if (receivers != null && receivers.Any())
            {
                if (streamId != Guid.Empty)
                {
                    int failedToTuneCount = 0;
                    foreach (var receiver in receivers)
                    {

                        if (await receiver.TuneAsync(frequency))
                        {
                            _logger.LogInformation($"Enabling stream {streamId} on receiver {receiver.Id} for {frequency}mhz");
                            var stream = new RssiStream(streamId, receiver.Id, ReceiverType, _timeProvider.ElapsedMilliseconds, receiver.TunedFrequency, receiver.AssignedFrequency);
                            receiver.CurrentStream = stream;
                            OnReceiversChanged?.Invoke(this, new EventArgs());
                            return stream;
                        }
                        else
                        {
                            failedToTuneCount++;
                        }
                        break;
                    }
                    if (failedToTuneCount > 0)
                    {
                        throw new InvalidOperationException("Available_receivers_failed_to_tune");
                    }
                }
                else
                {
                    throw new InvalidOperationException("Unable_to_start_stream_no_stream_id");
                }
            }
            throw new InvalidOperationException("No_receiver_available");

        }

        public void DisableStream(Guid streamId)
        {
            var receiver = _receivers.FirstOrDefault(x => x.CurrentStream?.Id == streamId);
            if (receiver != null)
            {
                _logger.LogInformation($"Disabling stream {streamId} on receiver {receiver.Id}");
                receiver.CurrentStream = null;
                OnReceiversChanged?.Invoke(this, new EventArgs());
            }
        }

    }
}
