using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using NaeTime.Client.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NaeTime.Client.Abstractions.Services;
using Toolbelt.Blazor.SpeechSynthesis;
using NaeTime.Shared.Client;

namespace NaeTime.Client.Services
{
    public class CommunicationService : ICommunicationService
    {
        public event EventHandler<FlightLapDtoEventArgs>? OnLapStarted;
        public event EventHandler<FlightLapDtoEventArgs>? OnLapCompleted;
        public event EventHandler<FlightSplitEventArgs>? OnSplitStarted;
        public event EventHandler<FlightSplitEventArgs>? OnSplitCompleted;
        public event EventHandler<RssiStreamReadingEventArgs>? OnRssiStreamReading;

        private HubConnection? _connection;
        private readonly NavigationManager _navigationManager;
        private readonly IAccessTokenProvider _tokenProvider;
        public CommunicationService(NavigationManager navigationManager, IAccessTokenProvider tokenProvider)
        {
            _navigationManager = navigationManager;
            _tokenProvider = tokenProvider;
        }

        public Task StartAsync()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(_navigationManager.ToAbsoluteUri("/clientHub"), options =>
                {
                    options.AccessTokenProvider = async () =>
                    {
                        var token = await _tokenProvider.RequestAccessToken();
                        if (token.Status == AccessTokenResultStatus.Success)
                        {
                            if (token.TryGetToken(out var access))
                            {
                                return access.Value;
                            }
                        }
                        return string.Empty;
                    };

                })
                .Build();
            HookUpMessageEvents(_connection);
            return _connection.StartAsync();
        }

        private void HookUpMessageEvents(HubConnection connection)
        {
            connection.On<LapDto>("OnStartedLap", lap =>
            {
                OnLapStarted?.Invoke(this, new FlightLapDtoEventArgs(lap));
            }); 
            connection.On<LapDto>("OnCompletedLap", lap =>
            {
                OnLapStarted?.Invoke(this, new FlightLapDtoEventArgs(lap));
            });
        }

        public Task StopAsync()
        {
            return _connection?.StopAsync() ?? Task.CompletedTask;
        }
    }
}
