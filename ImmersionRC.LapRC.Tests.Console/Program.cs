using ImmersionRC.LapRF;
using System.Net;

Console.WriteLine("Starting...");

var configuration = new LapRFDeviceConfiguration
{
    IPAddress = IPAddress.Parse("192.168.28.5"),
    Port = 5403
};

var communication = new LapRFEthernetCommunication();

await communication.ConnectAsync(configuration);

Console.WriteLine("Connected!");

var protocolFactory = new Lap();