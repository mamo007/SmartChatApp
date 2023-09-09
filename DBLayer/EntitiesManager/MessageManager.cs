using DALayer;
using DBLayer.Entities;
using DBLayer.EntitiesList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.EntitiesManager
{
    public interface IMessageManager
    {
        public MessageList GetAll();
        public MessageList GetMessageById(int id);
        public MessageList GetSentMessageId(string sender, string message, string date);
        public MessageList GetMessagesFromSenderToReceiver(string sender, string receiver);
        public Task<int> Add(Messages model);
        public Task<int> SoftDeleteById(int id);

    }
    public class MessageManager : IMessageManager
    {
        public MessageList GetAll()
        {
            string query = "select * from Messages";
            DataTable dt =  DBManager.ExecuteSelectDisconnected(query);

            return FromDataTableToMessageList(dt);
        }

        public MessageList GetMessageById(int id)
        {
            string query = $"select * from Messages where MessageId={id}";
            DataTable dt = DBManager.ExecuteSelectDisconnected(query);

            return FromDataTableToMessageList(dt);
        }

        public MessageList GetSentMessageId(string sender, string message, string date)
        {
            string query = $"select * from Messages where Sender='{sender}' AND Message =N'{message}' " +
                $"AND Date='{date}'";
            DataTable dt = DBManager.ExecuteSelectDisconnected(query);

            return FromDataTableToMessageList(dt);
        }

        public MessageList GetMessagesFromSenderToReceiver(string sender, string receiver)
        {
            string query = $"select * from Messages where Sender='{sender}' AND receiver ='{receiver}' " +
                $"OR Sender='{receiver}' AND receiver='{sender}' ORDER BY CONVERT(datetime2,Date)";
            DataTable dt = DBManager.ExecuteSelectDisconnected(query);

            return FromDataTableToMessageList(dt);
        }

        public async Task<int> Add(Messages model)
        {
            string query = $"insert into Messages values('{model.Sender}','{model.Receiver}',N'{model.Message}','{model.Date.ToString()}',{model.Read},{model.Delete})";
            return await DBManager.ExecuteNonQuery(query);
        }

        public async Task<int> SoftDeleteById(int id)
        {
            string query = $"update Messages set [Delete]=1 where MessageId={id}";
            return await DBManager.ExecuteNonQuery(query);
        }

        internal Messages FromDataRowToMessage(DataRow row)
        {
            Messages m = new Messages();
            //Mapping Part
            if (int.TryParse(row["MessageId"].ToString(), out int tempID))
                m.MessageId = tempID;
            m.Sender = row["Sender"].ToString();
            m.Receiver = row["Receiver"].ToString();
            m.Message = row["Message"].ToString();
            m.Date = row["Date"].ToString();
            if (int.TryParse(row["Read"].ToString(), out int tempID1))
                m.Read = tempID1;
            if (int.TryParse(row["Delete"].ToString(), out int tempID2))
                m.Delete = tempID2;

            return m;
        }
        internal MessageList FromDataTableToMessageList(DataTable table)
        {
            MessageList messageList = new MessageList();
            foreach (DataRow row in table.Rows)
            {
                messageList.Add(FromDataRowToMessage(row));
            }

            return messageList;
        }
    }
}
