using Microsoft.AspNetCore.Components;
using NaeTime.Client.Abstractions.Models;
using NaeTime.Client.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbelt.Blazor.SpeechSynthesis;

namespace NaeTime.Client.Components
{
    public partial class Notifications : ComponentBase
    {

        [Inject]
        public SpeechSynthesis? Speaker { get; set; }
        [Inject]
        public ICommunicationService? CommunicationsService { get; set; }

        protected override Task OnInitializedAsync()
        {

            if(CommunicationsService != null)
            {
                CommunicationsService.OnLapStarted += LapStarted;
                CommunicationsService.OnLapCompleted += LapCompleted;
            }

            return base.OnInitializedAsync();
        }

        private void LapStarted(object? sender, FlightLapDtoEventArgs e)
        {
            _ = Speaker?.SpeakAsync("Lap Started");
        }
        private void LapCompleted(object? sender, FlightLapDtoEventArgs e)
        {
            var lapTime = TimeSpan.FromMilliseconds((double)(e.Lap.EndTick - e.Lap.StartTick));

            _ = Speaker?.SpeakAsync($"{lapTime.TotalSeconds} seconds");
        }
    }
}
