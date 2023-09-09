using ChatCore.Services.MessageHandlers;
using DALayer;
using DBLayer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.EntitiesManager
{
    public interface IChatManager
    {
        /// <summary>
        /// Message handlers for user messages. Created on initialization.
        /// </summary>
        IList<IMessageHandler> MessageHandlers { get; set; }

        #region User Management

        /// <summary>
        /// Map Identity user to SignalR Id
        /// </summary>
        /// <param name="context">Hub context</param>
        Task ConnectUser(HubCallerContext context, IHubCallerClients clients);

        /// <summary>
        /// Remove User mappings before disconnect
        /// </summary>
        /// <param name="context">Hub context</param>
        /// <param name="clients">Client collection</param>
        Task DisconnectUser(HubCallerContext context, IHubCallerClients clients);

        /// <summary>
        /// Get`s user name from email
        /// </summary>
        /// <param name="context">Hub context</param>
        /// <param name="email">User email</param>
        /// <returns>Name before @ part</returns>
        string GetName(HubCallerContext context, string email = null);

        /// <summary>
        /// Finds user details in mappings
        /// </summary>
        /// <param name="userId">User Name or FullName or HubId or ConnectionId</param>
        /// <returns></returns>
        public UserDetail FindUser(string userId);
        /// <summary>
        /// check user online in mappings
        /// </summary>
        /// <param name="username">User Name or FullName or HubId or ConnectionId</param>
        /// <returns></returns>
        public string checkOnlineUser(string username);

        #endregion

        #region User Actions

        /// <summary>
        /// Notifications when user joins chat
        /// Could be done in OnConnectedAsync event
        /// </summary>
        /// <param name="context">Hub context</param>
        /// <param name="clients">Client collection</param>
        Task UserJoinedAsync(HubCallerContext context, IHubCallerClients clients);

        /// <summary>
        /// Handle user incoming messages
        /// </summary>
        /// <param name="context">Hub context</param>
        /// <param name="clients">Client collection</param>
        Task SendMessageAsync(HubCallerContext context, IHubCallerClients clients, string message);

        /// <summary>
        /// Gets connected users list formatted for chat output
        /// </summary>
        /// <param name="context">Hub context</param>
        /// <param name="clients">Client collection</param>
        /// <returns>User list formatted string</returns>
        string GetUsersListString(HubCallerContext context, IHubCallerClients clients);

        #endregion
    }
    public class ChatManager : IChatManager
    {
        public IList<IMessageHandler> MessageHandlers { get; set; } = new List<IMessageHandler>();

        private readonly IServiceProvider _serviceProvider;

        public ChatManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public string checkOnlineUser(string username)
        {
            var found = FindUser(username);
            string checkOnline = "";
            if (found == null)
                return checkOnline = "Offline";
            else
                return checkOnline = "Online";
        }

        private void FindMessageHandlers()
        {
            var handlers = typeof(ChatManager).Assembly
                .GetTypes()
                .Where(type => type.IsClass
                               && !type.IsAbstract
                               && type.IsPublic
                               && type.GetInterfaces().Any(i => i == typeof(IMessageHandler)))
                .ToList();

            foreach (var handler in handlers)
                MessageHandlers.Add(ActivatorUtilities.CreateInstance(_serviceProvider, handler) as IMessageHandler);

            // Ordering
            MessageHandlers = MessageHandlers.OrderBy(handler => handler.Order).ToList();
        }

        public static Dictionary<string, UserDetail> Users = new Dictionary<string, UserDetail>();

        public async Task ConnectUser(HubCallerContext context, IHubCallerClients clients)
        {
            lock (Users)
            {
                Users.Add(context.ConnectionId, new UserDetail()
                {
                    FullName = context.User.Identity.Name,
                    Name = GetName(context, context.User.Identity.Name),
                    HubUserId = context.UserIdentifier,
                    ConnectionId = context.ConnectionId,
                });
            }
            //User Online
            foreach(var item in Users)
            {
                await clients.All.SendAsync("Online", item.Value.Name);
            }
        }

        public async Task DisconnectUser(HubCallerContext context, IHubCallerClients clients)
        {
            lock (Users)
            {
                if (Users.ContainsKey(context.ConnectionId))
                    Users.Remove(context.ConnectionId);
            }

            // Get current user name
            var name = GetName(context);

            //User Offline
            await clients.Others.SendAsync("Offline", name);

            // Notify other users that current user left
            await clients.Others.SendAsync("ReceiveMessage", DateTime.Now.ToString("G"), null, $"{name} left!", 0, 0);
        }

        public string GetName(HubCallerContext context, string email = null)
        {
            email ??= context.User.Identity.Name;

            return email?.Split('@')[0];
        }

        public UserDetail FindUser(string userId)
        {
            lock (Users)
            {
                return Users.FirstOrDefault(user =>
                    user.Value.HubUserId.Equals(userId)
                    || user.Value.ConnectionId.Equals(userId)
                    || user.Value.Name.Equals(userId)
                    || user.Value.FullName.Equals(userId)
                ).Value;
            }
        }

        public async Task UserJoinedAsync(HubCallerContext context, IHubCallerClients clients)
        {
            var name = GetName(context);

            // Notify other users that someone is here
            await clients.Others.SendAsync("ReceiveMessage", DateTime.Now.ToString("G"), null, $"{name} is active now!", 0, 0);

            // Notify current user that we are ready
            //await clients.Caller.SendAsync("ReceiveMessage", DateTime.Now.ToString("G"), name, "is ready to chat!");
        }
        public async Task SendMessageAsync(HubCallerContext context, IHubCallerClients clients, string message)
        {
            if (!MessageHandlers.Any())
                lock (MessageHandlers)
                    if (!MessageHandlers.Any())
                        FindMessageHandlers();

            foreach (var handler in MessageHandlers)
            {
                if (await handler.Handle(context, clients, message))
                    break;
            }
        }

        public string GetUsersListString(HubCallerContext context, IHubCallerClients clients)
        {
            lock (Users)
            {
                return string.Join('\n', Users.Select(u => u.Value));
            }
        }
    }
}
