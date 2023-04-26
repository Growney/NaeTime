using NodaTime;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingCore.CommandHandler.ASPNET.Client
{
    public interface IHandlerClient
    {
        Task<HttpResponseMessage> SendCommandAsync(string identifier, object command, ZonedDateTime? validFrom = null, string authenticationToken = null, string address = null);

    }
}
