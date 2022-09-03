using Microsoft.AspNetCore.Components;
using NaeTime.Shared.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Client.Pages
{
    public partial class Tracks : ComponentBase
    {
        [Inject]
        public HttpClient? HttpClient { get; set; }
        private List<TrackDto>? _tracks = null;
        protected override async Task OnInitializedAsync()
        {
            if(HttpClient != null)
            {
                _tracks = await HttpClient.GetFromJsonAsync<List<TrackDto>>("track/all");

                await base.OnInitializedAsync();
            }
        }
    }
}
