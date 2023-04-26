using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.OpenIdConnect.Configuration
{
    public class OpenIdConnectOptions
    {
        public ProviderOptions ProviderOptions { get; set; } = new ProviderOptions();
        public PathOptions AuthenticationPaths { get; set; } = new PathOptions();
        public UserOptions UserOptions { get; set; } = new UserOptions();
    }
}
