using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Configuration.Abstractions
{
    public class OptionsClientConfigurationProvider<T> : IClientConfigurationProvider where T : class
    {
        public string Name { get; }
        private readonly IOptionsSnapshot<T> _config;
        public OptionsClientConfigurationProvider(string name, IOptionsSnapshot<T> config)
        {
            _config = config;
            Name = name;
        }
        public object GetConfiguration() => _config.Value;
    }
}
