using Core.Configuration.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Core.Configuration.Abstractions;

namespace Core.Configuration.Client
{
    public static class IConfigurationBuilderExtensionMethods
    {
        public static IConfigurationBuilder AddJsonHost(this IConfigurationBuilder builder, Uri baseAddress) => builder.AddJsonHost(baseAddress, ConfigurationConstants.DefaultPath);
        public static IConfigurationBuilder AddJsonHost(this IConfigurationBuilder builder, Uri baseAddress, string path)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            void GetStream(JsonStreamConfigurationSource source)
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = baseAddress;
                    HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, path);
                    var response = client.Send(message);
                    if (response.IsSuccessStatusCode)
                    {
                        source.Stream = response.Content.ReadAsStream();
                    }
                }
            }

            return builder.Add<JsonStreamConfigurationSource>(GetStream);
        }

        public static Task<IConfigurationBuilder> AddJsonHostAsync(this IConfigurationBuilder builder, Uri baseAddress) => builder.AddJsonHostAsync(baseAddress, ConfigurationConstants.DefaultPath);
        public static async Task<IConfigurationBuilder> AddJsonHostAsync(this IConfigurationBuilder builder, Uri baseAddress, string path)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            Stream stream = null;
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = baseAddress;
                var response = await client.GetAsync(path);
                if (response.IsSuccessStatusCode)
                {
                    stream = await response.Content.ReadAsStreamAsync();
                }
            }

            return builder.Add<JsonStreamConfigurationSource>(x => x.Stream = stream);
        }
    }
}
