using Newtonsoft.Json;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using System;
using System.Text;
using Core.Serialization.Abstractions;

namespace Core.Serialization.Json
{
    public class JsonParser : IParser
    {
        private readonly Encoding _encoding;
        private readonly JsonSerializerSettings _settings;
        public JsonParser(Encoding encoding = null, params JsonConverter[] converters)
        {
            _encoding = encoding ?? JsonSerializer.c_encoding;

            _settings = new JsonSerializerSettings();
            _settings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            for (int i = 0; i < converters.Length; i++)
            {
                _settings.Converters.Add(converters[i]);
            }
        }
        private void CheckData(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("Data");
            }
            if (data.Length == 0)
            {
                throw new ArgumentException("Data must not be empty");
            }
        }
        private void CheckData(string data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("Data");
            }
            if (data.Length == 0)
            {
                throw new ArgumentException("Data must not be empty");
            }
        }
        private T ParseData<T>(byte[] data)
        {
            CheckData(data);
            var jsonData = _encoding.GetString(data);
            return ParseData<T>(jsonData);
        }
        private T ParseData<T>(string data)
        {
            CheckData(data);
            try
            {
                return JsonConvert.DeserializeObject<T>(data, _settings);
            }
            catch (JsonReaderException ex)
            {
                throw new FormatException(ex.Message, ex);
            }
        }
        private bool TryParseData<T>(byte[] data, out T result)
        {
            try
            {
                CheckData(data);
                var jsonData = _encoding.GetString(data);
                return TryParse(jsonData, out result);
            }
            catch
            {
                result = default;
                return false;
            }
        }
        public T Parse<T>(byte[] data)
        {
            return ParseData<T>(data);
        }

        public object Parse(Type type, byte[] data)
        {
            CheckData(data);
            var jsonData = _encoding.GetString(data);
            return Parse(type, jsonData);
        }

        public bool TryParse<T>(byte[] data, out T result)
        {
            return TryParseData(data, out result);
        }

        public bool TryParse(Type type, byte[] data, out object result)
        {
            try
            {
                CheckData(data);
                var jsonData = _encoding.GetString(data);
                return TryParse(type, jsonData, out result);
            }
            catch
            {
                result = default;
                return false;
            }
        }

        public bool TryParse<T>(string data, out T result)
        {
            try
            {
                CheckData(data);
                result = JsonConvert.DeserializeObject<T>(data, _settings);
                return data != default;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        public T Parse<T>(string data)
        {
            return ParseData<T>(data);
        }

        public bool TryParse(Type t, string data, out object result)
        {
            try
            {
                CheckData(data);
                result = JsonConvert.DeserializeObject(data, t, _settings);
                return result != null;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        public object Parse(Type t, string data)
        {
            CheckData(data);
            try
            {
                return JsonConvert.DeserializeObject(data, t, _settings);
            }
            catch (JsonReaderException ex)
            {
                throw new FormatException(ex.Message, ex);
            }
        }

        public bool TryParse<T>(ReadOnlyMemory<byte> data, out T result)
        {
            try
            {
                var json = _encoding.GetString(data.Span);
                return TryParse(json, out result);
            }
            catch (Exception)
            {
                result = default;
                return false;
            }
        }

        public T Parse<T>(ReadOnlyMemory<byte> data)
        {
            var json = _encoding.GetString(data.Span);
            return Parse<T>(json);
        }

        public bool TryParse(Type t, ReadOnlyMemory<byte> data, out object result)
        {
            try
            {
                var json = _encoding.GetString(data.Span);
                return TryParse(t, json, out result);
            }
            catch (Exception)
            {
                result = default;
                return false;
            }
        }

        public object Parse(Type t, ReadOnlyMemory<byte> data)
        {
            var json = _encoding.GetString(data.Span);
            return Parse(t, json);
        }
    }
}
