using ImmersionRC.LapRF.Communication;
using ImmersionRC.LapRF.Protocol;
using System.Net;

Console.WriteLine("Starting...");



var communication = new LapRFEthernetCommunication(IPAddress.Parse("192.168.28.5"), 5403);

await communication.ConnectAsync();

Console.WriteLine("Connected!");

var statusProtocol = new StatusProtocol();
var passingRecordProtocol = new PassingRecordProtocol();
var radioFrequencySetupProtocol = new RadioFrequencySetupProtocol(communication);

var protocol = new LapRFProtocol(communication, statusProtocol, passingRecordProtocol, radioFrequencySetupProtocol);

var runningTask = protocol.RunAsync(CancellationToken.None);

Console.WriteLine("Getting transponder data");
var result = await radioFrequencySetupProtocol.GetSetupAsync(new byte[] { 1 }, CancellationToken.None);
foreach (var setup in result)
    Console.WriteLine($"Transponder {setup.TransponderId} is {(setup.IsEnabled ? "enabled" : "disabled")} on frequency {setup.Frequency}MHz with channel {setup.Channel} and band {setup.Band} and attenuation {setup.Attenuation}");

Console.WriteLine("Setting transponder data");
await radioFrequencySetupProtocol.SetupTransponderSlot(1, false, channel: 1, band: 2, attenuation: 60, frequencyInMHz: 5800);
Console.WriteLine("Getting transponder data");
result = await radioFrequencySetupProtocol.GetSetupAsync(new byte[] { 1 }, CancellationToken.None);


foreach (var setup in result)
    Console.WriteLine($"Transponder {setup.TransponderId} is {(setup.IsEnabled ? "enabled" : "disabled")} on frequency {setup.Frequency}MHz with channel {setup.Channel} and band {setup.Band} and attenuation {setup.Attenuation}");


await runningTask;