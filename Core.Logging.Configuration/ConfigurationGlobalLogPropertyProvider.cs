using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Core.Logging.Abstractions;
using Core.Logging.Core;

namespace Core.Logging.Configuration
{
    public class ConfigurationGlobalLogPropertyProvider : IGlobalLogPropertyProvider
    {
        private readonly List<(long flag, GlobalLogProperty property)> _values = new List<(long, GlobalLogProperty)>();

        public ConfigurationGlobalLogPropertyProvider(IConfiguration configuration)
        {
            if (configuration != null)
            {
                var configurationSection = configuration.GetSection("Logging:Properties");
                if (configurationSection != null)
                {
                    var values = configurationSection.Get<IEnumerable<GlobalLogProperty>>();
                    if (values != null)
                    {
                        foreach (var property in values)
                        {
                            var flag = property.Minimum.GetFlagValue(property.Maximum);
                            _values.Add((flag, property));
                        }
                    }
                }
            }
        }


        public IEnumerable<KeyValuePair<string, object>> GetProperties(LogLevel level)
        {
            foreach (var item in _values)
            {
                if (level.IncludedInFlag(item.flag))
                {
                    yield return new KeyValuePair<string, object>(item.property.Key, item.property.Value);
                }
            }
        }
    }
}
