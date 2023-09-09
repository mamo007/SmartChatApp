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
    public interface IUserManager
    {
        public UserList GetAll();
        public User GetByUsername(string username);
        public Task<int> Add(User model);
    }
    public class UserManager : IUserManager
    {
        public UserList GetAll()
        {
            string query = "select * from AspNetUsers";
            DataTable dt = DBManager.ExecuteSelectDisconnected(query);

            return FromDataTableToUserList(dt);
        }
        public User GetByUsername(string username)
        {
            string query = $"select * from AspNetUsers where UserName = '{username}'";
            DataTable dt = DBManager.ExecuteSelectDisconnected(query);

            return FromDataTableToUserList(dt)[0];
        }

        public async Task<int> Add(User model)
        {
            string query = $"insert into AspNetUsers values('{model.connectionId}','{model.UserName}','{model.NormalizedUserName}'" +
                $",'{model.Email}','{model.NormalizedEmail}',{model.EmailConfirmed},'{model.PasswordHash}','{model.SecurityStamp}'," +
                $"'{model.ConcurrencyStamp}','{model.PhoneNumber}',{model.PhoneNumberConfirmed},{model.TwoFactorEnabled},{model.LockoutEnd}," +
                $"{model.LockoutEnabled},{model.AccessFailedCount})";
            return await DBManager.ExecuteNonQuery(query);
        }

        internal User FromDataRowToUser(DataRow row)
        {
            User u = new User();
            //Mapping Part
            if (int.TryParse(row["Id"].ToString(), out int tempID))
                u.Id = tempID;
            u.connectionId = row["connectionId"].ToString();
            u.UserName = row["UserName"].ToString();
            u.NormalizedUserName = row["NormalizedUserName"].ToString();
            u.Email = row["Email"].ToString();
            u.NormalizedEmail = row["NormalizedEmail"].ToString();
            if (bool.TryParse(row["Id"].ToString(), out bool tempID1))
                u.EmailConfirmed = tempID1;
            u.PasswordHash = row["PasswordHash"].ToString();
            u.SecurityStamp = row["SecurityStamp"].ToString();
            u.ConcurrencyStamp = row["ConcurrencyStamp"].ToString();
            u.PhoneNumber = row["PhoneNumber"].ToString();
            if (bool.TryParse(row["PhoneNumberConfirmed"].ToString(), out bool tempID2))
                u.PhoneNumberConfirmed = tempID2;
            if (bool.TryParse(row["TwoFactorEnabled"].ToString(), out bool tempID3))
                u.TwoFactorEnabled = tempID3;
            if (DateTimeOffset.TryParse(row["LockoutEnd"].ToString(), out DateTimeOffset tempID4))
                u.LockoutEnd = tempID4;
            if (bool.TryParse(row["LockoutEnabled"].ToString(), out bool tempID5))
                u.LockoutEnabled = tempID5;
            if (int.TryParse(row["AccessFailedCount"].ToString(), out int tempID6))
                u.AccessFailedCount = tempID6;

            return u;
        }
        internal UserList FromDataTableToUserList(DataTable table)
        {
            UserList messageList = new UserList();
            foreach (DataRow row in table.Rows)
            {
                messageList.Add(FromDataRowToUser(row));
            }

            return messageList;
        }
    }
}
