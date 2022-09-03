using Microsoft.AspNetCore.Components;
using NaeTime.Shared.Client;
using System.Net.Http.Json;

namespace NaeTime.Client.Pages
{
    public partial class Track : ComponentBase
    {
        [Inject]
        public HttpClient? HttpClient { get; set; }
        private string? _error = null;
        public TrackDto? Dto { get; set; } = new TrackDto();
        [Parameter]
        public Guid? Id { get; set; }
        private List<NodeDto> _userNodes = new();


        protected override async Task OnInitializedAsync()
        {
            _error = null;
            try
            {
                if (HttpClient != null)
                {
                    var query = $"track";
                    if (Id.HasValue)
                    {
                        query = $"track/{Id}";
                    }
                    var track = await HttpClient.GetFromJsonAsync<TrackDto>(query);
                    if (track != null)
                    {
                        Dto = track;
                        var nodes = await HttpClient.GetFromJsonAsync<List<NodeDto>>("configuration/node");
                        if (nodes != null)
                        {
                            _userNodes = nodes;
                        }
                    }
                    else
                    {
                        _error = "Track Not Found";
                    }


                }
            }
            catch(Exception ex)
            {
                _error = ex.Message;
            }
            
            await base.OnInitializedAsync();
        }

        public void AddGate()
        {
            Dto?.Gates.Add(new TimedGateDto()
            {
                TrackId = Dto.Id,
                Position = Dto.Gates.Count,
            });
        }

        public List<NodeDto> GetAvailableNodes(TimedGateDto? dto)
        {
            var nodes = new List<NodeDto>();
            foreach (var node in _userNodes)
            {
                bool add = true;
                if (Dto != null)
                {
                    foreach (var gate in Dto.Gates)
                    {
                        if (dto == null || dto != gate)
                        {
                            if (node.Id == gate.NodeId)
                            {
                                add = false;
                                continue;
                            }
                        }
                    }
                }

                if (add)
                {
                    nodes.Add(node);
                }
            }
            return nodes;
        }

        public async void Save()
        {
            if(HttpClient != null)
            {
                await HttpClient.PostAsJsonAsync("/track", Dto);
            }
        }
    }
}
