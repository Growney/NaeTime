using System;
using System.Collections.Generic;
using System.Text;
using Core.Serialization.Abstractions;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using Newtonsoft.Json;

namespace Core.Serialization.Json
{
    public class JsonSerializer : ISerializer
    {
        public static readonly Encoding c_encoding = Encoding.UTF8;

        private readonly Encoding _encoding;
        private readonly JsonSerializerSettings _settings;
        public JsonSerializer(Encoding encoding = null, params JsonConverter[] converters)
        {
            _encoding = encoding ?? c_encoding;

            _settings = new JsonSerializerSettings();
            _settings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            for (int i = 0; i < converters.Length; i++)
            {
                _settings.Converters.Add(converters[i]);
            }
        }
        private byte[] SerializeObject(object data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("Data");
            }
            var jsonData = SerializeToString(data);
            var dataBytes = _encoding.GetBytes(jsonData);
            return dataBytes;
        }
        public byte[] Serialize(object data)
        {
            return SerializeObject(data);
        }

        public string SerializeToString(object data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("Data");
            }
            return JsonConvert.SerializeObject(data, _settings);
        }
    }
}

