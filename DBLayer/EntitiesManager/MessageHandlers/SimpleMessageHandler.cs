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
    /// Last handler, it just sends message to all other users. Never fails :D
    /// </summary>
    public class SimpleMessageHandler : IMessageHandler
    {
        public uint Order => 1000;

        private readonly IChatManager _chatService;
        private readonly IMessageManager _messageManager;

        public SimpleMessageHandler(IChatManager chatService, IMessageManager messageManager)
        {
            _chatService = chatService;
            _messageManager = messageManager;

        }

        public async Task<bool> Handle(HubCallerContext context, IHubCallerClients clients, string message)
        {
            Messages m = new Messages()
            {
                Date = DateTime.Now.ToString(),
                Delete = 0,
                Message = message,
                Read = 0,
                Receiver = "",
                Sender = _chatService.GetName(context),
            };
            await _messageManager.Add(m);

            //Handle MessageId
            MessageList sentMessage = _messageManager.GetSentMessageId(m.Sender, m.Message, m.Date);
            foreach (var item in sentMessage)
            {
                if (item != null)
                    await clients.All.SendAsync("ReceiveMessage", DateTime.Now.ToString("G"), _chatService.GetName(context), message, item.MessageId.ToString(), item.Read, item.Delete);
                else
                    await clients.All.SendAsync("ReceiveMessage", DateTime.Now.ToString("G"), _chatService.GetName(context), "an error occured!", 0, 0);
            }

            return true;
        }
    }
}