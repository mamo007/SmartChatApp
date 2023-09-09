using DBLayer.Entities;
using DBLayer.EntitiesList;
using DBLayer.EntitiesManager;
using Microsoft.AspNetCore.SignalR;
using System;

namespace SmartChatWebApp.Hubs
{
    public class ChatHub:Hub
    {
        #region Init

        private readonly IChatManager _chatService;
        private readonly IMessageManager _messageManager;

        public ChatHub(IChatManager chatService, IMessageManager messageManager)
        {
            _chatService = chatService;
            _messageManager = messageManager;
        }

        #endregion

        #region Identity User -> SignalR User Mappings

        public override Task OnConnectedAsync()
        {
            _chatService.ConnectUser(Context, Clients);

            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await _chatService.DisconnectUser(Context, Clients);

            await base.OnDisconnectedAsync(exception);
        }

        #endregion

        #region User Actions

        /// <summary>
        /// Massage sending and command handling
        /// </summary>
        /// <param name="message">User message</param>
        public async Task SendMessage(string message)
        {
            await _chatService.SendMessageAsync(Context, Clients, message);
        }

        /// <summary>
        /// Join notifications, could be moved to OnConneted
        /// </summary>
        public async Task Joined()
        {
            try
            {
                MessageList OldMessages = _messageManager.GetAll();
                foreach (var m in OldMessages)
                {
                    if(m.Receiver ==  "") // All Chat
                        await Clients.Caller.SendAsync("ReceiveMessage", m.Date, m.Sender, m.Message, m.MessageId.ToString(), m.Read, m.Delete);
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                await _chatService.UserJoinedAsync(Context, Clients);
            }
        }

        /// <summary>
        /// Handle Private Messages
        /// </summary>
        public async Task PrivateMessages(string receiver)
        {
            MessageList OldPvtMessages = _messageManager.GetMessagesFromSenderToReceiver(_chatService.GetName(Context), receiver);
            foreach (var m in OldPvtMessages)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", m.Date, m.Sender, m.Message, m.MessageId.ToString(), m.Read, m.Delete);
            }
        }

        /// <summary>
        /// Handle Soft Delete Messages
        /// </summary>
        public async Task SoftDeleteMessages(string id)
        {
            int messageId = int.Parse(id.Substring(id.LastIndexOf("-") + 2));
            MessageList Message = _messageManager.GetMessageById(messageId);
            foreach (var m in Message)
            {
                await _messageManager.SoftDeleteById(messageId);
                await Clients.All.SendAsync("SoftDeleteMessage", messageId, m.Date, m.Sender);
            }
        }
        #endregion
    }
}
