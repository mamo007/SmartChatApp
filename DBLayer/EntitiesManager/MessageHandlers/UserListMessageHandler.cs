﻿using System;
using System.Threading.Tasks;
using DBLayer.EntitiesManager;
using Microsoft.AspNetCore.SignalR;

namespace ChatCore.Services.MessageHandlers
{
    /// <summary>
    /// Prints all connected users for current user
    /// </summary>
    public class UserListMessageHandler : IMessageHandler
    {
        public uint Order => 900;

        private readonly IChatManager _chatService;

        public UserListMessageHandler(IChatManager chatService)
        {
            _chatService = chatService;
        }

        public async Task<bool> Handle(HubCallerContext context, IHubCallerClients clients, string message)
        {
            if (!message.Equals("/users", StringComparison.InvariantCultureIgnoreCase))
                return false;

            await clients.Caller.SendAsync("ReceiveMessage", DateTime.Now.ToString("G"), null, $"Users:\n{_chatService.GetUsersListString(context, clients)}", 0, 0);

            return true;
        }
    }
}