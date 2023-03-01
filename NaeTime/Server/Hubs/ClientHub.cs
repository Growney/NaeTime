using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using NaeTime.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Server.Hubs
{
    public class ClientHub : Hub
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ClientHub(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public override async Task OnConnectedAsync()
        {
            var user = await _userManager.GetUserAsync(Context.User);
            if(user != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Pilot-{user.PilotId}");
            }

            await base.OnConnectedAsync();
        }
    }
}
