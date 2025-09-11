using Discord;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sketch_Bot.Models;
using Discord.Rest;
using Newtonsoft.Json;
using System.IO;

namespace Sketch_Bot
{
    public class TempDB
    {
        private string server;
        private string database;
        private string username;
        private string password;
        private MySqlConnection? dbConnection;
        
        public TempDB()
        {
            Config config = Config.Load();
            server = config.TempDBHost;
            database = config.TempDBDatabase;
            username = config.TempDBUsername;
            password = config.TempDBPassword;
            MySqlConnectionStringBuilder stringBuilder = new MySqlConnectionStringBuilder();
            stringBuilder.Server = server;
            stringBuilder.UserID = username;
            stringBuilder.Password = password;
            stringBuilder.Database = database;
            stringBuilder.SslMode = MySqlSslMode.Disabled;
            stringBuilder.Pooling = false;
            stringBuilder.AllowPublicKeyRetrieval = true;


            var connectionString = stringBuilder.ToString();

            dbConnection = new MySqlConnection(connectionString);

            try
            {
                dbConnection.Open();
            }
            catch (Exception)
            {
                dbConnection = null;
            }
        }
        public MySqlDataReader FireCommand(string query)
        {
            if (dbConnection == null)
            {
                Console.WriteLine("Cant connect (FireCommand) TempDB");
                return null;
            }

            MySqlCommand command = new MySqlCommand(query, dbConnection);

            var mySqlReader = command.ExecuteReader();
            //Console.WriteLine($"Riskage Database: {queryCount}\n{query}");
            return mySqlReader;
        }
        public void CloseConnection()
        {
            if (dbConnection != null)
            {
                dbConnection.Close();
            }
        }
        public static void CloseAllConnections()
        {
            MySqlConnection.ClearAllPools();
        }
        public static void UpdateStats(BotStats stats)
        {
            var database = new TempDB(); /*Sets up a connection to the database*/
            try
            {
                var strings = string.Format("UPDATE `sketchstats` SET servers = {0}, users = '{1}', msg_since_startup = '{2}', msg_per_min = '{3}', startup_time = '{4}'", stats.Servers, stats.Users, stats.MsgSinceStartup, stats.MsgPerMin, $"{stats.StartUpTime.Year}-{stats.StartUpTime.Month}-{stats.StartUpTime.Day} {stats.StartUpTime.Hour}:{stats.StartUpTime.Minute}:{stats.StartUpTime.Second}"); /*This is your SQL string*/
                var reader = database.FireCommand(strings);/*fires the command*/
                reader.Close(); /*Closes the reader*/
                database.CloseConnection(); /*Closes the connection*/
            }
            catch(Exception ex)
            {
                database.CloseConnection(); /*Closes the connection*/
                Console.WriteLine(ex);
            }
            return;
        }
        public static void UpdateProfilePicture(string url1, string url2)
        {
            var database = new TempDB();
            try
            {
                var strings = string.Format("UPDATE `sketchstats` SET tao_avatar = '{0}', tjamp_avatar = '{1}'", url1, url2); /*This is your SQL string*/
                var reader = database.FireCommand(strings);/*fires the command*/
                reader.Close(); /*Closes the reader*/
                database.CloseConnection(); /*Closes the connection*/
            }
            catch(Exception ex)
            {
                database.CloseConnection();
                Console.WriteLine(ex);
            }
        }
    }
}
