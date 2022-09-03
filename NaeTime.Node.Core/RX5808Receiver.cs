using NaeTime.Node.Abstractions;
using NaeTime.Node.Abstractions.Models;
using RX5808;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Node.Core
{
    internal class RX5808Receiver : IRssiReceiver, IDisposable
    {
        public Guid Id { get; }
        public int AssignedFrequency { get; private set; }
        public int TunedFrequency { get; private set; }
        public RssiStream? CurrentStream { get; internal set; }

        private readonly RX5808Module _module;

        public RX5808Receiver(Guid id, RX5808Module module)
        {
            Id = id;
            _module = module ?? throw new ArgumentNullException(nameof(module));
        }
        public async Task InitialiseAsync()
        {
            if (!_module.IsInitialised)
            {
                _module.Initialise();
            }

            //TODO set module power options

            var moduleFrequency = await _module.GetActualStoredFrequencyAsync();

            AssignedFrequency = moduleFrequency;
            TunedFrequency = moduleFrequency;
        }
        public int GetRssi()
        {
            return _module.ReadRssi();
        }

        public async Task<bool> TuneAsync(int frequencyInMhz)
        {
            await _module.SetFrequencyAsync(frequencyInMhz);

            bool success = await _module.ConfirmFrequencyAsync(frequencyInMhz);

            AssignedFrequency = frequencyInMhz;

            TunedFrequency = await _module.GetActualStoredFrequencyAsync();

            return success;
        }

        public void Dispose()
        {
            _module.Dispose();
        }
    }
}
