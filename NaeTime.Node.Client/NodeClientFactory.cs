using NaeTime.Node.Client.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Node.Client
{
    public class NodeClientFactory : INodeClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public NodeClientFactory(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public INodeClient CreateClient(string nodeAddress)
        {
            if (!ValidateNodeAddress(nodeAddress))
            {
                throw new ArgumentException("Invalid_node_address");
            }

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(nodeAddress);

            return new NodeClient(client);
        }

        private static bool ValidateNodeAddress(string address)
        {
            //TODO proper address validation
            return true;
        }
    }
}
