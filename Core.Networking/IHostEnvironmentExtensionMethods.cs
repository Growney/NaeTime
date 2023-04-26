using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Net.Sockets;

namespace Core.Networking
{
    public static class IHostEnvironmentExtensionMethods
    {
        public static bool TryGetHostIP(this IHostEnvironment env, out string hostIP)
        {
            bool success = false;
            hostIP = string.Empty;
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    hostIP = endPoint.Address.ToString();

                }
                success = !string.IsNullOrWhiteSpace(hostIP);
            }
            catch
            {

            }
            if (!success)
            {
                try
                {
                    var host = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (var ip in host.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            hostIP = ip.ToString();

                            if (!string.IsNullOrWhiteSpace(hostIP))
                            {
                                break;
                            }
                        }
                    }

                    success = !string.IsNullOrWhiteSpace(hostIP);
                }
                catch
                {

                }
            }


            return success;


        }
    }
}
