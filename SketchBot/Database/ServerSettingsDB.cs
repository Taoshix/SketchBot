using Discord;
using Discord.WebSocket;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sketch_Bot.Models;
using Newtonsoft.Json;
using System.IO;

namespace Sketch_Bot
{
    public class ServerSettingsDB
    {

        private MySqlConnection? dbConnection;
        private Config config;
        public ServerSettingsDB()
        {
            config = Config.Load();

            MySqlConnectionStringBuilder stringBuilder = new MySqlConnectionStringBuilder();
            stringBuilder.Server = config.DatabaseHost;
            stringBuilder.UserID = config.DatabaseUsername;
            stringBuilder.Password = config.DatabasePassword;
            stringBuilder.Database = "Server settings";
            stringBuilder.SslMode = MySqlSslMode.Disabled;
            stringBuilder.Pooling = false;

            var connectionString = stringBuilder.ToString();

            dbConnection = new MySqlConnection(connectionString);

            try
            {
                dbConnection.Open();
            }
            catch (Exception ex)
            {
                dbConnection = null;
            }
        }
        public bool isDatabaseConnected()
        {
            var status = dbConnection != null;
            if (status)
            {
                dbConnection.Close();
            }
            return status;
        }
        public MySqlDataReader FireCommand(string query)
        {
            if (dbConnection == null)
            {
                Console.WriteLine("Cant connect (FireCommand) Server Settings");
                return null;
            }

            MySqlCommand command = new MySqlCommand(query, dbConnection);

            var mySqlReader = command.ExecuteReader();
            //Console.WriteLine($"ServerSettings Database: {queryCount}\n{query}");
            return mySqlReader;
        }
        public void CloseConnection()
        {
            if (dbConnection != null)
            {
                dbConnection.Close();
            }
        }
        public static void CreateTableRole(ulong guildid)
        {
            var database = new ServerSettingsDB();
            var str = string.Format("CREATE TABLE IF NOT EXISTS `{0}_roles` (roleId varchar(50), level int, PRIMARY KEY (roleId))", guildid);
            var table = database.FireCommand(str);

            database.CloseConnection();
        }
        public static void CreateTableWords(ulong guildid)
        {
            var database = new ServerSettingsDB();
            var str = string.Format("CREATE TABLE IF NOT EXISTS `{0}_words` (words text)", guildid);
            var table = database.FireCommand(str);

            database.CloseConnection();
        }
        public static void MakeSettings(ulong guildid, int levelup)
        {
            var database = new Database();
            var str = string.Format("INSERT INTO `server_settings` (id, prefix, welcomechannel, modlogchannel, xpmultiplier, LevelupMessages) VALUES ({0}, '?', 0, 0, 1, {1})", guildid, levelup);
            var table = database.FireCommand(str);

            database.CloseConnection();
        }
        public static void UpdateAllTables(ulong guilid)
        {
            var database = new ServerSettingsDB();
            var str = string.Format("ALTER TABLE `{0}` ADD COLUMN `LevelupMessages` INT NOT NULL DEFAULT '1' AFTER `xpmultiplier`; ", guilid);
            var table = database.FireCommand(str);

            database.CloseConnection();
        }
        public static void UpdateXprate(ulong guildid, int rate)
        {
            var database = new Database();
            var str = string.Format("UPDATE `server_settings` SET xpmultiplier = '{1}' WHERE id = {0}", guildid, rate);
            var table = database.FireCommand(str);

            database.CloseConnection();

            return;
        }
        public static void AddWord(ulong guildid, string words)
        {
            var database = new ServerSettingsDB();
            var str = string.Format("INSERT INTO `{0}_words` (words) VALUES ('{1}')", guildid, words);
            var table = database.FireCommand(str);

            database.CloseConnection();

            
        }
        public static void AddRole(ulong guildid, ulong roleId, int level)
        {
            var database = new ServerSettingsDB();
            var str = string.Format("INSERT INTO `{0}_roles` (roleId, level) VALUES ('{1}', {2})", guildid, roleId, level);
            var table = database.FireCommand(str);

            database.CloseConnection();

            
        }
        public static void RemoveRole(ulong guildid, ulong roleId)
        {
            var database = new ServerSettingsDB();
            var str = string.Format("DELETE FROM `{0}_roles` WHERE roleId = '{1}'", guildid, roleId);
            var table = database.FireCommand(str);

            database.CloseConnection();

            
        }
        public static void DelWord(ulong guildid, string words)
        {
            var database = new ServerSettingsDB();
            var str = string.Format("DELETE FROM `{0}_words` WHERE words = ('{1}')", guildid, words);
            var table = database.FireCommand(str);

            database.CloseConnection();

            
        }
        public static void UpdatePrefix(ulong guildid, string newprefix)
        {
            var database = new Database();
            var str = string.Format("UPDATE `server_settings` SET prefix = ('{1}') WHERE id = {0}", guildid, newprefix);
            var table = database.FireCommand(str);

            database.CloseConnection();

            return;
        }
        public static List<Serversettings> GetSettings(ulong guildid)
        {
            var result = new List<Serversettings>();
            var database = new Database();
            var str = $"SELECT * FROM `server_settings` WHERE id = {guildid}";
            var table = database.FireCommand(str);

            while (table.Read())
            {
                var prefix = table["prefix"] == DBNull.Value ? string.Empty : (string)table["prefix"];
                var welcomechannel = table["welcomechannel"] == DBNull.Value ? 0 : (ulong)table["welcomechannel"];
                var modlogchannel = table["modlogchannel"] == DBNull.Value ? 0 : (ulong)table["modlogchannel"];
                var xprate = table["xpmultiplier"] == DBNull.Value ? 1 : (int)table["xpmultiplier"];
                var levelupmessages = table["LevelupMessages"] == DBNull.Value ? 1 : (ulong)table["LevelupMessages"];

                result.Add(new Serversettings
                {
                    Prefix = prefix,
                    ModlogChannel = modlogchannel,
                    WelcomeChannel = welcomechannel,
                    XpMultiplier = xprate,
                    LevelupMessages = levelupmessages == 1
                });
            }

            database.CloseConnection();
            return result;
        }
        public static void UpdateLevelupMessagesBool(ulong guildid, int boool)
        {
            var database = new ServerSettingsDB();
            var str = string.Format("UPDATE `server_settings` SET LevelupMessages = ('{1}') WHERE id = {0}", guildid, boool);
            var table = database.FireCommand(str);

            database.CloseConnection();

            return;
        }
        public static List<string> CheckPrefix(SocketUser user)
        {
            var socketguild = (user as SocketGuildUser).Guild;
            var result = new List<string>();
            var database = new Database();
            var str = string.Format("SELECT * FROM `server_settings` WHERE id = {0}", socketguild.Id.ToString());
            var userTable = database.FireCommand(str);

            while (userTable.Read())
            {
                var prefix = (string)userTable["prefix"];

                result.Add(prefix);
            }
            database.CloseConnection();
            return result;
        }
        public static void SetWelcomeChannel(ulong id, ulong guildid)
        {
            var database = new Database();
            var str = string.Format("UPDATE `server_settings` SET welcomechannel = '{1}' WHERE id = {0}", guildid, id);
            var table = database.FireCommand(str);

            database.CloseConnection();

            
        }
        public static void SetModlogChannel(ulong id, ulong guildid)
        {
            var database = new Database();
            var str = string.Format("UPDATE `server_settings` SET modlogchannel = '{1}' WHERE id = {0}", guildid, id);
            var table = database.FireCommand(str);

            database.CloseConnection();

            
        }
        public static List<Serversettings> GetRole(ulong guildid, long level)
        {
            var result = new List<Serversettings>();
            var database = new ServerSettingsDB();
            var str = string.Format("SELECT * FROM `{0}_roles` WHERE level = '{1}'", guildid, level);
            var table = database.FireCommand(str);

            while (table.Read())
            {
                var roleId = (ulong)table["roleId"];

                result.Add(new Serversettings
                {
                    roleId = roleId,
                });
            }

            database.CloseConnection();

            return result;
        }
        public static List<Serversettings> GetRoles(ulong guildid)
        {
            var result = new List<Serversettings>();
            var database = new ServerSettingsDB();
            var str = string.Format("SELECT * FROM `{0}_roles`", guildid);
            var table = database.FireCommand(str);

            while (table.Read())
            {
                var roleId = (ulong)table["roleId"];
                var roleLevel = (int)table["level"];

                result.Add(new Serversettings
                {
                    roleId = roleId,
                    roleLevel = roleLevel
                });
            }

            database.CloseConnection();

            return result;
        }
        public static List<Serversettings> GetWords(ulong guildid)
        {
            var result = new List<Serversettings>();
            var database = new ServerSettingsDB();
            var str = string.Format("SELECT * FROM `{0}_words`", guildid);
            var table = database.FireCommand(str);

            while (table.Read())
            {
                var words = (string)table["words"];

                result.Add(new Serversettings
                {
                    Words = words,
                });
            }

            database.CloseConnection();

            return result;
        }
    }
}