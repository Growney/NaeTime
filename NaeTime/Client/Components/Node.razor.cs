using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using NaeTime.Client.Shared;
using NaeTime.Shared.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Client.Components
{
    [Authorize]
    public partial class Node : ComponentBase
    {
        [Inject]
        public HttpClient? HttpClient { get; set; }
        [Parameter]
        public NodeDto Dto { get; set; } = new NodeDto();

        private readonly List<RX5808Dto> _receivers = new List<RX5808Dto>();

        private string? _error = null;
        protected override void OnParametersSet()
        {
            _receivers.Clear();
            if (Dto?.RX5808Receivers != null)
            {
                _receivers.AddRange(Dto.RX5808Receivers);
            }

            if (Dto != null)
            {
                Dto.RX5808Receivers = _receivers;
            }
        }

        public async void Save()
        {
            StateHasChanged();
            _error = null;
            if (HttpClient != null)
            {
                var response = await HttpClient.PostAsJsonAsync("configuration/node", Dto);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _error = error;
                    StateHasChanged();
                }
            }
        }

        public void AddRX5808()
        {
            _receivers.Add(new RX5808Dto());
        }

        public void RemoveRX5808(RX5808Dto receiver)
        {
            _receivers.Remove(receiver);
        }
    }
}
