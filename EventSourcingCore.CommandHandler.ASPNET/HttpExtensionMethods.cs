using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Core.Serialization.Json;
using EventSourcingCore.CommandHandler.Abstractions;

namespace EventSourcingCore.CommandHandler.ASPNET
{
    public static class HttpExtensionMethods
    {
        public static Task<ICommand> GetCommand(this HttpRequest request, Type type)
        {
            return GetCommand(request.Body, type);
        }
        public static async Task<ICommand> GetCommand(Stream body, Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            string requestContent;
            using (StreamReader streamReader = new StreamReader(body, Encoding.UTF8))
            {
                requestContent = await streamReader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(requestContent))
            {
                throw new InvalidOperationException("No command body provided");
            }

            JsonParser parser = new JsonParser();
            object commandObject = parser.Parse(type, requestContent);

            return commandObject as ICommand;
        }
    }
}
