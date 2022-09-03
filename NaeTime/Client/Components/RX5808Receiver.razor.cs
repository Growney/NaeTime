using Microsoft.AspNetCore.Components;
using NaeTime.Client.Shared;
using NaeTime.Shared.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Client.Components
{
    public partial class RX5808Receiver : ComponentBase
    {
        [Parameter]
        public Action<RX5808Dto>? OnRemove { get; set; }
        [Parameter]
        public RX5808Dto Dto { get; set; } = new RX5808Dto();
    }
}
