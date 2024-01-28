using NaeTime.Client.Razor.Lib.Models;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Client.MAUI.Lib;
public class Test : ISubscriber
{
    public Task When(Pilot message)
    {

        return Task.CompletedTask;
    }
}
