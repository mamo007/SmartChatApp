using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DALayer
{
    public class DBManager
    {
        public static DataTable ExecuteSelectDisconnected(string query)
        {
            SqlDataAdapter adapter = new SqlDataAdapter(query, GetConnectionString());
            DataTable dt = new DataTable();
            //await Task.Run(() => adapter.Fill(dt));
            adapter.Fill(dt);

            return dt;
        }

        public static async Task<int> ExecuteNonQuery(string query)
        {
            SqlConnection connection = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand(query, connection);
            connection.Open();
            int AffectedRows = await cmd.ExecuteNonQueryAsync();
            connection.Close();

            return AffectedRows;
        }

        public static object ExecuteScalar(string query)
        {
            SqlConnection connection = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand(query, connection);
            connection.Open();
            object FirstColRow = cmd.ExecuteScalar();
            connection.Close();

            return FirstColRow;
        }
        public static object ExecuteInsertScalar(string query)
        {
            SqlConnection connection = new SqlConnection(GetConnectionString());
            SqlCommand cmd = new SqlCommand($"{query};select @@identity;", connection);
            connection.Open();
            object FirstColRow = cmd.ExecuteScalar();
            connection.Close();

            return FirstColRow;
        }

        private static IConfiguration Configuration { get; set; }
        public static string GetConnectionString()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            return Configuration.GetConnectionString("SmartChatConn");
        }
    }
}