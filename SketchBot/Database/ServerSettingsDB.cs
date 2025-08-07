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

        private MySqlConnection dbConnection;
        private Config config;
        public ServerSettingsDB()
        {
            config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));

            MySqlConnectionStringBuilder stringBuilder = new MySqlConnectionStringBuilder();
            stringBuilder.Server = config.DatabaseHost;
            stringBuilder.UserID = config.DatabaseUsername;
            stringBuilder.Password = config.DatabasePassword;
            stringBuilder.Database = "Server settings";
            stringBuilder.SslMode = MySqlSslMode.Disabled;
            stringBuilder.Pooling = false;

            var connectionString = stringBuilder.ToString();

            dbConnection = new MySqlConnection(connectionString);

            var ping = dbConnection.Ping();
            if (ping)
            {
                dbConnection.Open();
            }
            else
            {
                Console.WriteLine("Cant connect to database");
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
                Console.WriteLine("Cant connect (FireCommand)");
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
        public static void CreateTable(string guildid)
        {
            var database = new ServerSettingsDB();
            var str = string.Format("CREATE TABLE IF NOT EXISTS `{0}` (prefix varchar(50) DEFAULT '?', welcomechannel varchar(50) DEFAULT NULL, modlogchannel varchar(50) DEFAULT NULL, xpmultiplier int DEFAULT 1, LevelupMessages int DEFAULT 1)", guildid);
            var table = database.FireCommand(str);

            database.CloseConnection();
        }
        public static void CreateTableRole(string guildid)
        {
            var database = new ServerSettingsDB();
            var str = string.Format("CREATE TABLE IF NOT EXISTS `{0}_roles` (roleId varchar(50), level int, PRIMARY KEY (roleId))", guildid);
            var table = database.FireCommand(str);

            database.CloseConnection();
        }
        public static void CreateTableWords(string guildid)
        {
            var database = new ServerSettingsDB();
            var str = string.Format("CREATE TABLE IF NOT EXISTS `{0}_words` (words text)", guildid);
            var table = database.FireCommand(str);

            database.CloseConnection();
        }
        public static void MakeSettings(string guildid, int levelup)
        {
            var database = new ServerSettingsDB();
            var str = string.Format("INSERT INTO `{0}` (prefix, welcomechannel, modlogchannel, xpmultiplier, LevelupMessages) VALUES ('?', NULL, NULL, 1, {1})", guildid, levelup);
            var table = database.FireCommand(str);

            database.CloseConnection();
        }
        public static void UpdateAllTables(string guilid)
        {
            var database = new ServerSettingsDB();
            var str = string.Format("ALTER TABLE `{0}` ADD COLUMN `LevelupMessages` INT NOT NULL DEFAULT '1' AFTER `xpmultiplier`; ", guilid);
            var table = database.FireCommand(str);

            database.CloseConnection();
        }
        public static void UpdateXprate(string guildid, int rate)
        {
            var database = new ServerSettingsDB();
            var str = string.Format("UPDATE `{0}` SET xpmultiplier = '{1}'", guildid, rate);
            var table = database.FireCommand(str);

            database.CloseConnection();

            return;
        }
        public static void AddWord(string guildid, string words)
        {
            var database = new ServerSettingsDB();
            var str = string.Format("INSERT INTO `{0}_words` (words) VALUES ('{1}')", guildid, words);
            var table = database.FireCommand(str);

            database.CloseConnection();

            
        }
        public static void AddRole(string guildid, string roleId, int level)
        {
            var database = new ServerSettingsDB();
            var str = string.Format("INSERT INTO `{0}_roles` (roleId, level) VALUES ('{1}', {2})", guildid, roleId, level);
            var table = database.FireCommand(str);

            database.CloseConnection();

            
        }
        public static void RemoveRole(string guildid, string roleId)
        {
            var database = new ServerSettingsDB();
            var str = string.Format("DELETE FROM `{0}_roles` WHERE roleId = '{1}'", guildid, roleId);
            var table = database.FireCommand(str);

            database.CloseConnection();

            
        }
        public static void DelWord(string guildid, string words)
        {
            var database = new ServerSettingsDB();
            var str = string.Format("DELETE FROM `{0}_words` WHERE words = ('{1}')", guildid, words);
            var table = database.FireCommand(str);

            database.CloseConnection();

            
        }
        public static void UpdatePrefix(string guildid, string newprefix)
        {
            var database = new ServerSettingsDB();
            var str = string.Format("UPDATE `{0}` SET prefix = ('{1}')", guildid, newprefix);
            var table = database.FireCommand(str);

            database.CloseConnection();

            return;
        }
        public static List<Serversettings> GetSettings(string guildid)
        {
            var result = new List<Serversettings>();
            var database = new ServerSettingsDB();
            var str = string.Format("SELECT * FROM `{0}`", guildid);
            var table = database.FireCommand(str);

            while (table.Read())
            {
                var prefix = (string)table["prefix"];
                var welcomechannel = (string)table["welcomechannel"];
                var modlogchannel = (string)table["modlogchannel"];
                var xprate = (int)table["xpmultiplier"];

                result.Add(new Serversettings
                {
                    Prefix = prefix,
                    ModlogChannel = modlogchannel,
                    WelcomeChannel = welcomechannel,
                    XpMultiplier = xprate,
                });
            }

            database.CloseConnection();

            return result;
        }
        public static List<Serversettings> GetLevelupMessageBool(string guildid)
        {
            var result = new List<Serversettings>();
            var database = new ServerSettingsDB();
            var str = string.Format("SELECT * FROM `{0}`", guildid);
            var table = database.FireCommand(str);

            while (table.Read())
            {
                var levelupmessages = (int)table["LevelupMessages"];
                result.Add(new Serversettings
                {
                    LevelupMessages = levelupmessages
                });
            }

            database.CloseConnection();

            return result;
        }
        public static void UpdateLevelupMessagesBool(string guildid, int boool)
        {
            var database = new ServerSettingsDB();
            var str = string.Format("UPDATE `{0}` SET LevelupMessages = ('{1}')", guildid, boool);
            var table = database.FireCommand(str);

            database.CloseConnection();

            return;
        }
        public static List<Serversettings> GetModlogChannel(string guildid)
        {
            var result = new List<Serversettings>();
            var database = new ServerSettingsDB();
            var str = string.Format("SELECT * FROM `{0}`", guildid);
            var table = database.FireCommand(str);

            while (table.Read())
            {
                var modlogchannel = (string)table["modlogchannel"];

                result.Add(new Serversettings
                {
                    ModlogChannel = modlogchannel,
                });
            }

            database.CloseConnection();

            return result;
        }
        public static List<Serversettings> GetPrefix(string guildid)
        {
            var result = new List<Serversettings>();
            var database = new ServerSettingsDB();
            var str = string.Format("SELECT * FROM `{0}`", guildid);
            var table = database.FireCommand(str);

            while (table.Read())
            {
                var prefix = (string)table["prefix"];

                result.Add(new Serversettings
                {
                    Prefix = prefix,
                });
            }

            database.CloseConnection();

            return result;
        }
        public static List<String> CheckPrefix(SocketUser user)
        {
            var socketguild = (user as SocketGuildUser).Guild;
            var result = new List<String>();
            var database = new ServerSettingsDB();
            var str = string.Format("SELECT * FROM `{0}`", socketguild.Id.ToString());
            var userTable = database.FireCommand(str);

            while (userTable.Read())
            {
                var prefix = (string)userTable["prefix"];

                result.Add(prefix);
            }
            database.CloseConnection();
            return result;
        }
        public static void SetWelcomeChannel(string id, string guildid)
        {
            var database = new ServerSettingsDB();
            var str = string.Format("UPDATE `{0}` SET welcomechannel = '{1}'", guildid, id);
            var table = database.FireCommand(str);

            database.CloseConnection();

            
        }
        public static void SetModlogChannel(string id, string guildid)
        {
            var database = new ServerSettingsDB();
            var str = string.Format("UPDATE `{0}` SET modlogchannel = '{1}'", guildid, id);
            var table = database.FireCommand(str);

            database.CloseConnection();

            
        }
        public static List<Serversettings> GetWelcomeChannel(string guildid)
        {
            var result = new List<Serversettings>();
            var database = new ServerSettingsDB();
            var str = string.Format("SELECT * FROM `{0}`", guildid);
            var table = database.FireCommand(str);

            while (table.Read())
            {
                var welcomechannel = (string)table["welcomechannel"];

                result.Add(new Serversettings
                {
                    WelcomeChannel = welcomechannel,
                });
            }

            database.CloseConnection();

            return result;
        }
        public static List<Serversettings> GetXpRate(string guildid)
        {
            var result = new List<Serversettings>();
            var database = new ServerSettingsDB();
            var str = string.Format("SELECT * FROM `{0}`", guildid);
            var table = database.FireCommand(str);

            while (table.Read())
            {
                var xprate = (int)table["xprate"];

                result.Add(new Serversettings
                {
                    XpMultiplier = xprate,
                });
            }

            database.CloseConnection();

            return result;
        }
        public static List<Serversettings> GetRole(string guildid, long level)
        {
            var result = new List<Serversettings>();
            var database = new ServerSettingsDB();
            var str = string.Format("SELECT * FROM `{0}_roles` WHERE level = '{1}'", guildid, level);
            var table = database.FireCommand(str);

            while (table.Read())
            {
                var roleId = (string)table["roleId"];

                result.Add(new Serversettings
                {
                    roleId = roleId,
                });
            }

            database.CloseConnection();

            return result;
        }
        public static List<Serversettings> GetRoles(string guildid)
        {
            var result = new List<Serversettings>();
            var database = new ServerSettingsDB();
            var str = string.Format("SELECT * FROM `{0}_roles`", guildid);
            var table = database.FireCommand(str);

            while (table.Read())
            {
                var roleId = (string)table["roleId"];
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
        public static List<Serversettings> GetWords(string guildid)
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