using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DBLayer.Entities;
using DBLayer.EntitiesList;
using DBLayer.EntitiesManager;
using Microsoft.AspNetCore.SignalR;

namespace ChatCore.Services.MessageHandlers
{
    /// <summary>
    /// Handles private messages between users
    /// </summary>
    public class PrivateMessageHandler : IMessageHandler
    {
        public uint Order => 100;

        private readonly IChatManager _chatService;
        private readonly IMessageManager _messageManager;
        private readonly IUserManager _userManager;

        public PrivateMessageHandler(IChatManager chatService, IMessageManager messageManager, IUserManager userManager)
        {
            _chatService = chatService;
            _messageManager = messageManager;
            _userManager = userManager;
        }

        public async Task<bool> Handle(HubCallerContext context, IHubCallerClients clients, string message)
        {
            if (!message.StartsWith("/pm:", StringComparison.InvariantCultureIgnoreCase))
                return false;

            //                                     /pm: grp[1] grp[2]
            // Bit of Regex, message is in format '/pm: userId message'
            var regex = new Regex(@"^\/pm\:\s*([^\s]+)\s+(.*)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            var match = regex.Match(message);
            if (match.Success)
            {
                // Lets try to send message

                var userId = match.Groups[1].Value;
                    
                // Find our user
                var found = _chatService.FindUser(userId);
                var foundindb = _userManager.GetByUsername(userId + "@gmail.com");

                //Online User
                if (found != null)
                {
                    userId = found.HubUserId;
                    var userName = found.Name;
                    

                    Messages m = new Messages()
                    {
                        Date = DateTime.Now.ToString(),
                        Delete = 0,
                        Message = match.Groups[2].Value,
                        Read = 0,
                        Receiver = userName,
                        Sender = _chatService.GetName(context),
                    };
                    await _messageManager.Add(m);

                    //Handle MessageId
                    MessageList sentMessage = _messageManager.GetSentMessageId(m.Sender, m.Message, m.Date);
                    foreach (var item in sentMessage)
                    {
                        if (item != null)
                            await clients.Users(userId, context.UserIdentifier).SendAsync("ReceiveMessage", DateTime.Now.ToString("G"), $"{_chatService.GetName(context)}", match.Groups[2].Value, item.MessageId.ToString(), m.Read, m.Delete);
                        else
                            await clients.Users(userId, context.UserIdentifier).SendAsync("ReceiveMessage", DateTime.Now.ToString("G"), $"{_chatService.GetName(context)}", "an error occured!", 0, 0);
                    }

                    // Success
                    return true;
                }
                else if(foundindb != null) //Offline User
                {
                    Messages m = new Messages()
                    {
                        Date = DateTime.Now.ToString(),
                        Delete = 0,
                        Message = match.Groups[2].Value,
                        Read = 0,
                        Receiver = userId,
                        Sender = _chatService.GetName(context),
                    };
                    await _messageManager.Add(m);

                    //Handle MessageId
                    MessageList sentMessage = _messageManager.GetSentMessageId(m.Sender, m.Message, m.Date);
                    foreach(var item in sentMessage)
                    {
                        if(item != null)
                            await clients.Users(userId, context.UserIdentifier).SendAsync("ReceiveMessage", DateTime.Now.ToString("G"), $"{_chatService.GetName(context)}", match.Groups[2].Value, item.MessageId.ToString(), m.Read, m.Delete);
                        else
                           await clients.Users(userId, context.UserIdentifier).SendAsync("ReceiveMessage", DateTime.Now.ToString("G"), $"{_chatService.GetName(context)}", "an error occured!", 0, 0);
                    }

                    // Success
                    return true;
                }
            }

            // Fail
            // Notify current user we don`t understand
            await clients.Caller.SendAsync("ReceiveMessage", DateTime.Now.ToString("G"), null, "Wrong command or user :/", 0, 0);

            return true; // We don`t want other users notice even if we fail in sending private message
        }
    }
}